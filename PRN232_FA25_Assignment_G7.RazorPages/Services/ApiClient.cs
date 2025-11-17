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

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"POST {endpoint} failed with status {response.StatusCode}: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
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

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("GET {Endpoint} failed: {StatusCode} - {Error}", endpoint, response.StatusCode, errorContent);
            throw new HttpRequestException(
                $"GET {endpoint} failed with status {response.StatusCode}: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
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

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"PUT {endpoint} failed with status {response.StatusCode}: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
    }

    /// <summary>
    /// Generic DELETE request
    /// </summary>
    public async Task<bool> DeleteAsync(string endpoint)
    {
        var httpRequest = CreateRequest(HttpMethod.Delete, endpoint);
        var response = await _httpClient.SendAsync(httpRequest);

        return response.IsSuccessStatusCode;
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
}
