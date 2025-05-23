using AuthorizationService;
using IdentityService.Api.Endpoints;
using IdentityService.Api.ServiceCollectionExtensions;
using IdentityService.Application.Handlers;
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

builder.Services.AddRazorPages();

// 🔹 Настраиваем Serilog + Seq
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.Seq("http://localhost:8001");
});

//🔹 Настройка автомаппера
builder.Services.ConfigureAutoMapper();

// 🔹 Настройка базы данных
builder.Services.ConfigureDb(builder.Configuration);

// 🔹 Настраиваем Identity
builder.Services.ConfigureIdentity();
builder.Services.ConfigureAuthorization();

// 🔹 Настраиваем OpenIddict
builder.Services.ConfigureOpenIddict();

// 🔹 Добавляем медиатр
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblyContaining<DiMediatrDetector>();
});

// 🔹 Добавляем валидатор
builder.Services.ConfigureFluentValidator();

// 🔹 Добавляем HostedService
builder.Services.ConfigureHostedService();

// Build Application
var app = builder.Build();

// Open Api
app.ConfigureScalar();



// 🔹 Регистрация middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
// Регистрация Endpoints
app.ConfigureEndpoints();


app.Run();
