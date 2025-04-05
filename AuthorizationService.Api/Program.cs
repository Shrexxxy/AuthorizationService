using AuthorizationService;
using AuthorizationService.Api.Endpoints;
using AuthorizationService.Application.Handlers;
using AuthorizationService.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Настройка сервисов
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

// 🔹 Настраиваем Serilog + Seq
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.Seq("http://localhost:8001");
});

// 🔹 Настройка базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                              "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=Microservice";
    
    options.UseNpgsql(connectionString, builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name!))
        .UseOpenIddict<Guid>();
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
    options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
});

// 🔹 Настраиваем Identity
builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// 🔹 Настраиваем OpenIddict
builder.Services.AddOpenIddict()
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
            .RequireProofKeyForCodeExchange()
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

// Добавляе медиатр
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblyContaining<DiMediatrDetector>();
});


// Build Application
var app = builder.Build();

// Open Api
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Identity API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithDarkMode(true);
    });
}

// 🔹 Регистрация middleware
app.UseRouting();
app.UseAuthentication();     
app.UseAuthorization();
// Регистрация Endpoints
app.UseEndpoints(endpoints =>     
{
    endpoints.MapAuthEndpoints();
    endpoints.MapApplicationsEndpoints();
});


app.Run();
