using AuthorizationService.DAL;
using IdentityService.Api.ServiceCollectionExtensions.OpenIdDict;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.LoginPath = "/connect/login";
                    options.Cookie.MaxAge = TimeSpan.FromDays(1);
                    options.Cookie.IsEssential = true;
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthData.AuthenticationSchemes, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });
        // services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        // services.AddSingleton<IAuthorizationHandler, AppPermissionHandler>();
        
        return services;
    }
}