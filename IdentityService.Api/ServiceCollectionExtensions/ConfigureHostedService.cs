using IdentityService.Infrastructure.Workers;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHostedService(this IServiceCollection services)
    {
        services.AddHostedService<RoleSeeder>();

        return services;
    }
}
