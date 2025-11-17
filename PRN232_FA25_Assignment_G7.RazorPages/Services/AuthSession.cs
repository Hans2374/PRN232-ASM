using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PRN232_FA25_Assignment_G7.RazorPages.Services;

/// <summary>
/// Manages JWT token storage and retrieval using HttpOnly cookies and session
/// </summary>
public class AuthSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthSession> _logger;
    private const string JwtTokenKey = "JwtToken";
    private const string JwtCookieName = "PRN232.JWT";

    public AuthSession(IHttpContextAccessor httpContextAccessor, ILogger<AuthSession> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Save JWT token to both session and HttpOnly cookie
    /// </summary>
    public void SaveToken(string token)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Store in session for server-side access
        httpContext.Session.SetString(JwtTokenKey, token);

        // Store in HttpOnly cookie for additional security
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Allow HTTP for development (localhost:5001)
            SameSite = SameSiteMode.Lax, // Changed from Strict to Lax for better compatibility
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        };

        httpContext.Response.Cookies.Append(JwtCookieName, token, cookieOptions);
    }

    /// <summary>
    /// Retrieve JWT token from session or cookie
    /// </summary>
    public string? GetToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext is null - cannot retrieve token");
            return null;
        }

        // Try to get from session first
        var token = httpContext.Session.GetString(JwtTokenKey);
        
        if (!string.IsNullOrEmpty(token))
        {
            _logger.LogDebug("Token retrieved from session (length: {Length})", token.Length);
            return token;
        }
        
        // Fallback to cookie if session doesn't have it
        _logger.LogDebug("Token not in session, checking cookie...");
        httpContext.Request.Cookies.TryGetValue(JwtCookieName, out token);
        
        if (!string.IsNullOrEmpty(token))
        {
            _logger.LogInformation("Token found in cookie, restoring to session (length: {Length})", token.Length);
            // Restore to session if found in cookie
            httpContext.Session.SetString(JwtTokenKey, token);
            return token;
        }

        _logger.LogWarning("No JWT token found in session or cookie");
        return null;
    }

    /// <summary>
    /// Clear JWT token from both session and cookie
    /// </summary>
    public void ClearToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Clear session
        httpContext.Session.Remove(JwtTokenKey);

        // Clear cookie
        httpContext.Response.Cookies.Delete(JwtCookieName);
    }

    /// <summary>
    /// Extract role claim from JWT token
    /// </summary>
    public string? GetRoleFromToken()
    {
        var token = GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.Role || 
                c.Type == "role" || 
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            
            return roleClaim?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extract user ID from JWT token
    /// </summary>
    public Guid? GetUserIdFromToken()
    {
        var token = GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.NameIdentifier || 
                c.Type == "sub" ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if token exists and is not expired
    /// </summary>
    public bool IsAuthenticated()
    {
        var token = GetToken();
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            // Check if token is expired
            return jwtToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }
}
