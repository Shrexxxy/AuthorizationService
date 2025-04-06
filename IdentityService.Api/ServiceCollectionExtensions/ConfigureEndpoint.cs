using IdentityService.Api.Endpoints;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static void ConfigureEndpoints(this WebApplication app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // Регистрация маршрутов на уровне приложения
        app.MapAuthEndpoints();
        app.MapApplicationsEndpoints();
    }
}
