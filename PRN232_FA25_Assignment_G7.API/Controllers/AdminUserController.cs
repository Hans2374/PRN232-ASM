using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FA25_Assignment_G7.Services.DTOs.Users;
using PRN232_FA25_Assignment_G7.Services.Interfaces;

namespace PRN232_FA25_Assignment_G7.API.Controllers;

/// <summary>
/// Admin-only user management endpoints
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<AdminUserController> _logger;

    public AdminUserController(
        IUserManagementService userManagementService,
        ILogger<AdminUserController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    /// <response code="200">Returns list of users</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not Admin</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userManagementService.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} users", users.Count());
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return BadRequest(new { message = "An error occurred while retrieving users." });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If user not found</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not Admin</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var user = await _userManagementService.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new { message = $"User with ID '{id}' not found." });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return BadRequest(new { message = "An error occurred while retrieving the user." });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation details</param>
    /// <returns>Created user details</returns>
    /// <response code="201">User successfully created</response>
    /// <response code="400">If username already exists or request is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not Admin</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Invalid user creation request");
                return BadRequest(new { message = "Invalid request. Username and password are required." });
            }

            var user = await _userManagementService.CreateAsync(request);

            _logger.LogInformation("User {Username} created successfully with ID {UserId}", user.Username, user.Id);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user
            );
        }
        catch (PRN232_FA25_Assignment_G7.Services.Exceptions.DuplicateUsernameException ex)
        {
            _logger.LogWarning("User creation conflict: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid user creation data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { message = "An error occurred while creating the user." });
        }
    }

    /// <summary>
    /// Update user information (Admin can modify role, email, full name, active status)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update details</param>
    /// <returns>Updated user details</returns>
    /// <response code="200">User successfully updated</response>
    /// <response code="404">If user not found</response>
    /// <response code="400">If request is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not Admin</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (request == null)
            {
                _logger.LogWarning("Invalid user update request for ID {UserId}", id);
                return BadRequest(new { message = "Invalid request." });
            }

            var user = await _userManagementService.UpdateAsync(id, request);

            _logger.LogInformation("User {UserId} updated successfully", id);

            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User update failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid user update data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return BadRequest(new { message = "An error occurred while updating the user." });
        }
    }

    /// <summary>
    /// Soft delete user (sets IsActive to false)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    /// <response code="204">User successfully deleted</response>
    /// <response code="404">If user not found</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not Admin</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _userManagementService.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                return NotFound(new { message = $"User with ID '{id}' not found." });
            }

            _logger.LogInformation("User {UserId} soft deleted successfully", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return BadRequest(new { message = "An error occurred while deleting the user." });
        }
    }
}
