namespace PRN232_FA25_Assignment_G7.Services.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    
    // Legacy property for backward compatibility
    public string Secret
    {
        get => Key;
        set => Key = value;
    }
    
    public int ExpiryMinutes
    {
        get => ExpirationMinutes;
        set => ExpirationMinutes = value;
    }
}
