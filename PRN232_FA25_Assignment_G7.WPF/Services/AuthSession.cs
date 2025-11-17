namespace PRN232_FA25_Assignment_G7.WPF.Services;

public static class AuthSession
{
    public static string? JwtToken { get; set; }
    public static string? Role { get; set; }
    public static string? Username { get; set; }
    public static string? FullName { get; set; }
    public static Guid? UserId { get; set; }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

    public static CurrentUserInfo? CurrentUser => IsAuthenticated 
        ? new CurrentUserInfo { FullName = FullName, Role = Role, Username = Username } 
        : null;

    // Role-based helpers
    public static bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
    public static bool IsManager => Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true;
    public static bool IsMod => Role?.Equals("Mod", StringComparison.OrdinalIgnoreCase) == true || 
                                Role?.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
    public static bool IsExaminer => Role?.Equals("Examiner", StringComparison.OrdinalIgnoreCase) == true;

    public static bool HasRole(params string[] roles)
    {
        if (string.IsNullOrEmpty(Role)) return false;
        return roles.Any(r => Role.Equals(r, StringComparison.OrdinalIgnoreCase));
    }

    public static void Clear()
    {
        JwtToken = null;
        Role = null;
        Username = null;
        FullName = null;
        UserId = null;
    }

    public static void SetSession(string token, string username, string role, string fullName, Guid userId)
    {
        JwtToken = token;
        Username = username;
        Role = role;
        FullName = fullName;
        UserId = userId;
    }
}

public class CurrentUserInfo
{
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? Username { get; set; }
}
