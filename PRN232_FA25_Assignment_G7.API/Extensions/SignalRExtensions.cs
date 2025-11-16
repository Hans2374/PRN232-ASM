namespace PRN232_FA25_Assignment_G7.API.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRConfiguration(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
