using AuthorizationService.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityService.Infrastructure.Workers;

public class RoleSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public RoleSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Инициализация ролей на старте приложения
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await SeedRolesAsync(roleManager);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Ничего не требуется на завершении
        return Task.CompletedTask;
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Список ролей из UserRoles
        var roles = new[] 
        {
            UserRoles.SuperAdmin,
            UserRoles.Admin,
            UserRoles.User
        };

        foreach (var role in roles)
        {
            // Добавляем роль, если она ещё не существует
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
