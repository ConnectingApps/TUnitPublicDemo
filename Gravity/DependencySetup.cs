using Gravity.Interfaces;
using Gravity.Services;

namespace Gravity;

public static class DependencySetup
{
    public static IServiceCollection AddGravity(this IServiceCollection services)
    {
        services.AddSingleton<IGravityConfiguration, GravityConfiguration>();
        services.AddScoped<IGravityCalculator, GravityCalculator>();
        return services;
    }
}