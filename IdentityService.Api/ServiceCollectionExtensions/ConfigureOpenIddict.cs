using AuthorizationService.DAL;
using OpenIddict.Abstractions;

namespace IdentityService.Api.ServiceCollectionExtensions;


public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureOpenIddict(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token");
                options.SetAuthorizationEndpointUris("/connect/authorize");

                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow();

                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.OfflineAccess);

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough();
        
                // сертификат разработки
                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}
