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

// 🔹 Настраиваем Serilog + Seq
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.Seq("http://localhost:8001");
});

// 🔹 Настройка базы данных
builder.Services.ConfigureDb(builder.Configuration);

// 🔹 Настраиваем Identity
builder.Services.ConfigureIdentity();

// 🔹 Настраиваем OpenIddict
builder.Services.ConfigureOpenIddict();

// Добавляе медиатр
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblyContaining<DiMediatrDetector>();
});


// Build Application
var app = builder.Build();

// Open Api
app.ConfigureScalar();

// 🔹 Регистрация middleware
app.UseRouting();
app.UseAuthentication();     
app.UseAuthorization();

// Регистрация Endpoints
app.ConfigureEndpoints();


app.Run();
