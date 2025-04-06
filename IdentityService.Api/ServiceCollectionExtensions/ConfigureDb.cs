using AuthorizationService.DAL;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            string connectionString = 
                configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=Microservice";

            options.UseNpgsql(connectionString, builder => 
                    builder.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name!))
                .UseOpenIddict<Guid>();
        });

        return services;
    }
}
