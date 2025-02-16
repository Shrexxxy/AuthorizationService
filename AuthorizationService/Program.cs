using AuthorizationService;
using AuthorizationService.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using IdentityRole = Microsoft.AspNetCore.Identity.IdentityRole;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Настраиваем Serilog + Seq
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.Seq("http://localhost:5341");
});

// 🔹 Настройка базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                              "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=Microservice";
    
    options.UseNpgsql(connectionString, builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name!)).UseOpenIddict<Guid>();
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
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddUserStore<UserStore>()
    .AddRoleStore<RoleStore<IdentityRole<Guid>, AppDbContext, Guid>>()
    .AddUserManager<UserManager<IdentityUser<Guid>>>()
    .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<IdentityUser<Guid>, IdentityRole<Guid>>>()
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

        options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.OfflineAccess);

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// 🔹 Настраиваем MudBlazor
builder.Services.AddMudServices();

// 🔹 Добавляем Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Включаем свагер
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Используем OpenIddict и авторизацию
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Выносим API в отдельный класс
app.UseEndpoints(endpoints => endpoints.MapAuthEndpoints());

// 🔹 Запуск Blazor Server
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
