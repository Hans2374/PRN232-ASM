using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PRN232_FA25_Assignment_G7.Services.Configuration;
using PRN232_FA25_Assignment_G7.Services.Helpers;
using System.Text;

namespace PRN232_FA25_Assignment_G7.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        services.AddSingleton(jwtSettings);
        services.AddSingleton<JwtTokenGenerator>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };

            // SignalR support
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Admin", "Manager"));
            options.AddPolicy("ModeratorOrAbove", policy => policy.RequireRole("Admin", "Manager", "Moderator"));
            options.AddPolicy("ExaminerOrAbove", policy => policy.RequireRole("Admin", "Manager", "Moderator", "Examiner"));
        });

        return services;
    }
}
