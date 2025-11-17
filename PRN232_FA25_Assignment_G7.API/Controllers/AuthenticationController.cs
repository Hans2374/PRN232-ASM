using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.Services.DTOs.Auth;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

/// <summary>
/// Authentication and user management endpoints
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthService authService,
        ILogger<AuthenticationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    /// <param name="request">Login credentials (username and password)</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">Returns the authentication token and user details</response>
    /// <response code="400">If credentials are invalid or request is malformed</response>
    /// <response code="401">If user is not authorized</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with invalid request data");
                return BadRequest(new { message = "Invalid request. Username and password are required." });
            }

            _logger.LogInformation("Login attempt for username: {Username}", request.Username);

            var response = await _authService.LoginAsync(request);

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}. Reason: {Reason}", 
                request?.Username ?? "unknown", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request?.Username ?? "unknown");
            return BadRequest(new { message = "An error occurred during login. Please try again." });
        }
    }

    /// <summary>
    /// Register a new user (Admin only)
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <returns>Registration response with user ID and username</returns>
    /// <response code="201">User successfully created</response>
    /// <response code="400">If username already exists or request is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not authorized (not Admin)</response>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Registration attempt with invalid request data");
                return BadRequest(new { message = "Invalid request. Username and password are required." });
            }

            _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

            var response = await _authService.RegisterAsync(request);

            _logger.LogInformation("User {Username} registered successfully with role {Role}", 
                response.Username, response.Role);

            return CreatedAtAction(
                nameof(GetCurrentUser),
                new { },
                response
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for username: {Username}. Reason: {Reason}", 
                request?.Username ?? "unknown", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid registration data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", 
                request?.Username ?? "unknown");
            return BadRequest(new { message = "An error occurred during registration. Please try again." });
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details from JWT token</returns>
    /// <response code="200">Returns the current user information</response>
    /// <response code="401">If user is not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        try
        {
            // Extract claims from JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
            var username = User.FindFirst("username")?.Value 
                           ?? User.Identity?.Name;
            var email = User.FindFirst(ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Unable to extract user information from token");
                return Unauthorized(new { message = "Invalid token claims" });
            }

            var response = new
            {
                UserId = userId,
                Username = username,
                Email = email ?? string.Empty,
                Role = role ?? string.Empty
            };

            _logger.LogDebug("Current user info retrieved for: {Username}", username);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return BadRequest(new { message = "An error occurred while retrieving user information." });
        }
    }

    /// <summary>
    /// Health check endpoint for authentication service
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            Service = "Authentication",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
