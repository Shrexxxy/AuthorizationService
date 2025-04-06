using IdentityService.Application.Mapping;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DiAutoMapperDetector));
        return services;
    }
}
