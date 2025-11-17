using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PRN232_FA25_Assignment_G7.WPF.Models;

namespace PRN232_FA25_Assignment_G7.WPF.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5000";

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void SetAuthorizationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"API call failed: {ex.Message}", ex);
        }
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"API call failed: {ex.Message}", ex);
        }
    }

    public async Task<T?> PutAsync<T>(string url, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"API call failed: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string url)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new Exception($"API call failed: {ex.Message}", ex);
        }
    }

    // User Management API Methods
    public async Task<List<UserResponse>> GetUsersAsync()
    {
        SetAuthorizationToken(AuthSession.JwtToken ?? string.Empty);
        var result = await GetAsync<List<UserResponse>>("/api/admin/users");
        return result ?? new List<UserResponse>();
    }

    public async Task<UserResponse?> CreateUserAsync(CreateUserRequest request)
    {
        SetAuthorizationToken(AuthSession.JwtToken ?? string.Empty);
        return await PostAsync<UserResponse>("/api/admin/users", request);
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        SetAuthorizationToken(AuthSession.JwtToken ?? string.Empty);
        return await PutAsync<UserResponse>($"/api/admin/users/{id}", request);
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        SetAuthorizationToken(AuthSession.JwtToken ?? string.Empty);
        return await DeleteAsync($"/api/admin/users/{id}");
    }
}
