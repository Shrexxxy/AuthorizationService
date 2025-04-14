using IdentityService.Api.Options;
using Scalar.AspNetCore;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static void ConfigureScalar(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            var identityConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("identitysetting.json")
                .Build();
            
            var identitySettings = identityConfiguration.GetSection("IdentitySettings").Get<IdentitySettings>();
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("Identity API")
                    .WithTheme(ScalarTheme.DeepSpace)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .WithDarkMode(true);
                
                options
                    .WithPreferredScheme("OAuth2") // Security scheme name from the OpenAPI document
                    .WithOAuth2Authentication(oauth =>
                    {
                        oauth.ClientId = identitySettings.ClientId;
                        oauth.Scopes = identitySettings.Scopes.ToArray();
                    });
            });
        }
    }

}
