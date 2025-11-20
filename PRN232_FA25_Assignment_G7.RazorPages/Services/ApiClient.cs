using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PRN232_FA25_Assignment_G7.RazorPages.Models;

namespace PRN232_FA25_Assignment_G7.RazorPages.Services;

/// <summary>
/// Centralized API client for making HTTP requests to the backend API
/// Automatically handles JWT token injection and JSON serialization
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, AuthSession authSession, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _authSession = authSession;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        _logger.LogInformation("ApiClient initialized with BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
    }

    /// <summary>
    /// Create a HttpRequestMessage and attach Authorization header for this request only
    /// </summary>
    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, endpoint);

        var token = _authSession.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("Auth header present for {Method} {Endpoint}: Yes", method, endpoint);
        }
        else
        {
            _logger.LogWarning("Auth header present for {Method} {Endpoint}: No (token missing)", method, endpoint);
        }

        if (content != null)
        {
            request.Content = content;
        }
        return request;
    }

    /// <summary>
    /// Generic POST request with request/response body
    /// </summary>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = CreateRequest(HttpMethod.Post, endpoint, content);
        var response = await _httpClient.SendAsync(httpRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            // Surface a typed ApiException so callers can react to status codes (e.g., 409 Conflict)
            throw new ApiException(response.StatusCode, responseContent);
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
    }

    /// <summary>
    /// Generic GET request
    /// </summary>
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var fullUrl = new Uri(_httpClient.BaseAddress!, endpoint);
        _logger.LogInformation("GET Request to: {Url}", fullUrl);
        var httpRequest = CreateRequest(HttpMethod.Get, endpoint);
        var response = await _httpClient.SendAsync(httpRequest);

        _logger.LogInformation("GET {Endpoint} - Status: {StatusCode}", endpoint, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Resource not found: {Endpoint}", endpoint);
                return default;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Unauthorized access to {Endpoint} - JWT may be invalid or missing", endpoint);
                throw new UnauthorizedAccessException($"Unauthorized access to {endpoint}. Please login again.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogError("Forbidden access to {Endpoint} - User does not have required role", endpoint);
                throw new UnauthorizedAccessException($"Access denied. You do not have permission to access this resource.");
            }

            _logger.LogError("GET {Endpoint} failed: {StatusCode} - {Error}", endpoint, response.StatusCode, responseContent);
            throw new ApiException(response.StatusCode, responseContent);
        }

        _logger.LogDebug("GET {Endpoint} - Response length: {Length} bytes", endpoint, responseContent.Length);
        return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
    }

    /// <summary>
    /// Generic PUT request with request/response body
    /// </summary>
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpRequest = CreateRequest(HttpMethod.Put, endpoint, content);
        var response = await _httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(response.StatusCode, responseContent);
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
    }

    /// <summary>
    /// Generic DELETE request
    /// </summary>
    public async Task<bool> DeleteAsync(string endpoint)
    {
        var httpRequest = CreateRequest(HttpMethod.Delete, endpoint);
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException(response.StatusCode, errorContent);
        }

        return true;
    }

    // ==================== Authentication Endpoints ====================

    /// <summary>
    /// Check if API is reachable
    /// </summary>
    public async Task<bool> IsApiReachableAsync()
    {
        try
        {
            var fullUrl = new Uri(_httpClient.BaseAddress!, "swagger/index.html");
            _logger.LogInformation("Testing API connectivity at: {Url}", fullUrl);
            
            var response = await _httpClient.GetAsync("swagger/index.html");
            var isReachable = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
            
            _logger.LogInformation("API reachable: {IsReachable}, Status: {StatusCode}", isReachable, response.StatusCode);
            return isReachable;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is NOT reachable at {BaseAddress}", _httpClient.BaseAddress);
            return false;
        }
    }

    /// <summary>
    /// Login to the API and return authentication response
    /// </summary>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var fullUrl = new Uri(_httpClient.BaseAddress!, "/api/auth/login");
        _logger.LogInformation("Attempting login at: {Url}", fullUrl);
        _logger.LogDebug("Login request: Username={Username}", request.Username);
        
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            _logger.LogDebug("Request JSON: {Json}", json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/login", content);
            
            _logger.LogInformation("Login response status: {StatusCode}", response.StatusCode);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Login failed: Invalid credentials");
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Login failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Login failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Login response: {Response}", responseContent);
            
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions);
            
            if (authResponse != null)
            {
                _logger.LogInformation("Login successful for user: {Username}, Role: {Role}", authResponse.Username, authResponse.Role);
            }
            
            return authResponse;
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
        {
            _logger.LogError(ex, "Connection refused: API is not running at {BaseAddress}", _httpClient.BaseAddress);
            throw new InvalidOperationException(
                $"Cannot connect to API server at {_httpClient.BaseAddress}. Please ensure the API is running.", ex);
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw authentication errors as-is
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during login");
            throw new InvalidOperationException(
                $"Failed to communicate with API at {_httpClient.BaseAddress}. Error: {ex.Message}", ex);
        }
    }

    // ==================== User Management Endpoints ====================

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    public async Task<List<UserResponse>> GetUsersAsync()
    {
        var result = await GetAsync<List<UserResponse>>("/api/admin/users");
        return result ?? new List<UserResponse>();
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        return await GetAsync<UserResponse>($"/api/admin/users/{id}");
    }

    /// <summary>
    /// Create new user (Admin only)
    /// </summary>
    public async Task<UserResponse?> CreateUserAsync(CreateUserRequest request)
    {
        return await PostAsync<CreateUserRequest, UserResponse>("/api/admin/users", request);
    }

    /// <summary>
    /// Update existing user (Admin only)
    /// </summary>
    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        return await PutAsync<UpdateUserRequest, UserResponse>($"/api/admin/users/{id}", request);
    }

    /// <summary>
    /// Soft delete user (Admin only)
    /// </summary>
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        return await DeleteAsync($"/api/admin/users/{id}");
    }

    /// <summary>
    /// Get raw HTTP response for streaming (e.g., file downloads)
    /// </summary>
    public async Task<HttpResponseMessage> GetRawAsync(string endpoint)
    {
        var httpRequest = CreateRequest(HttpMethod.Get, endpoint);
        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"GET {endpoint} failed with status {response.StatusCode}: {errorContent}");
        }

        return response;
    }

    // ==================== Moderator Endpoints ====================

    /// <summary>
    /// Get moderator dashboard data
    /// </summary>
    public async Task<ModeratorDashboardResponse?> GetModeratorDashboardAsync()
    {
        return await GetAsync<ModeratorDashboardResponse>("/api/moderator/dashboard");
    }

    /// <summary>
    /// Get paged list of complaints
    /// </summary>
    public async Task<PagedResult<ComplaintSummaryDto>?> GetModeratorComplaintsAsync(
        string? status = "Pending", Guid? examId = null, string? studentCode = null, int page = 1, int pageSize = 25)
    {
        var query = $"?status={status}&page={page}&pageSize={pageSize}";
        if (examId.HasValue) query += $"&examId={examId}";
        if (!string.IsNullOrEmpty(studentCode)) query += $"&studentCode={studentCode}";

        return await GetAsync<PagedResult<ComplaintSummaryDto>>($"/api/moderator/complaints{query}");
    }

    /// <summary>
    /// Get complaint details
    /// </summary>
    public async Task<ComplaintDetailDto?> GetModeratorComplaintAsync(Guid id)
    {
        return await GetAsync<ComplaintDetailDto>($"/api/moderator/complaints/{id}");
    }

    /// <summary>
    /// Decide on a complaint
    /// </summary>
    public async Task<bool> DecideModeratorComplaintAsync(Guid id, DecisionDto dto)
    {
        await PostAsync<DecisionDto, object>($"/api/moderator/complaints/{id}/decision", dto);
        return true;
    }

    /// <summary>
    /// Get paged list of zero-score submissions
    /// </summary>
    public async Task<PagedResult<ZeroScoreSubmissionDto>?> GetZeroScoreSubmissionsAsync(
        Guid? examId = null, string? status = "Pending", string? studentCode = null, int page = 1, int pageSize = 25)
    {
        var query = $"?status={status}&page={page}&pageSize={pageSize}";
        if (examId.HasValue) query += $"&examId={examId}";
        if (!string.IsNullOrEmpty(studentCode)) query += $"&studentCode={studentCode}";

        return await GetAsync<PagedResult<ZeroScoreSubmissionDto>>($"/api/moderator/zero-scores{query}");
    }

    /// <summary>
    /// Get zero-score submission details
    /// </summary>
    public async Task<ZeroScoreDetailDto?> GetZeroScoreDetailAsync(Guid id)
    {
        return await GetAsync<ZeroScoreDetailDto>($"/api/moderator/zero-scores/{id}");
    }

    /// <summary>
    /// Verify zero-score submission
    /// </summary>
    public async Task<bool> VerifyZeroScoreAsync(Guid id, VerifyZeroScoreRequest request)
    {
        await PostAsync<VerifyZeroScoreRequest, object>($"/api/moderator/zero-scores/{id}/verify", request);
        return true;
    }

    // Examiner API methods
    public async Task<ExaminerDashboardResponse?> GetExaminerDashboardAsync()
    {
        return await GetAsync<ExaminerDashboardResponse>("/api/examiner/dashboard");
    }

    public async Task<PagedResult<SubmissionListDto>?> GetExaminerSubmissionsAsync(SubmissionFilter filter)
    {
        var queryParams = new List<string>();
        if (filter.ExamId.HasValue)
            queryParams.Add($"examId={filter.ExamId}");
        if (!string.IsNullOrEmpty(filter.Status))
            queryParams.Add($"status={filter.Status}");
        queryParams.Add($"page={filter.PageNumber}");
        queryParams.Add($"pageSize={filter.PageSize}");

        var queryString = string.Join("&", queryParams);
        return await GetAsync<PagedResult<SubmissionListDto>>($"/api/examiner/submissions?{queryString}");
    }

    public async Task<SubmissionDetailDto?> GetExaminerSubmissionDetailAsync(Guid id)
    {
        return await GetAsync<SubmissionDetailDto>($"/api/examiner/submissions/{id}");
    }

    public async Task<bool> SubmitExaminerGradeAsync(Guid id, SubmitGradeRequest request)
    {
        await PostAsync<SubmitGradeRequest, object>($"/api/examiner/submissions/{id}/grade", request);
        return true;
    }

    public async Task<PagedResult<DoubleGradingTaskDto>?> GetDoubleGradingTasksAsync(DoubleGradingFilter filter)
    {
        var queryParams = new List<string>
        {
            $"page={filter.PageNumber}",
            $"pageSize={filter.PageSize}"
        };

        var queryString = string.Join("&", queryParams);
        return await GetAsync<PagedResult<DoubleGradingTaskDto>>($"/api/examiner/double-grading?{queryString}");
    }

    public async Task<DoubleGradingDetailDto?> GetDoubleGradingDetailAsync(Guid id)
    {
        return await GetAsync<DoubleGradingDetailDto>($"/api/examiner/double-grading/{id}");
    }

    public async Task<bool> SubmitDoubleGradingAsync(Guid id, SubmitDoubleGradingRequest request)
    {
        await PostAsync<SubmitDoubleGradingRequest, object>($"/api/examiner/double-grading/{id}/grade", request);
        return true;
    }
}
