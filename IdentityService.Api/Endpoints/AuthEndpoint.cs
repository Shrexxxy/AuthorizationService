using System.Security.Claims;
using IdentityService.Application.Model;
using IdentityService.Application.Query.Account;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IdentityService.Api.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/auth");

        api.MapPost("/register", RegisterAsync)
            .WithOpenApi()
            .WithDescription("Регистрация пользователя.")
            .AllowAnonymous();
    
        api.MapPost("/login", LoginAsync)
            .WithOpenApi()
            .WithDescription("Авторизация пользователя.")
            .AllowAnonymous();
    
        api.MapPost("/refresh", RefreshTokenAsync)
            .WithOpenApi()
            .WithDescription("Обновление токена.")
            .AllowAnonymous();
    }

    //TODO: временно
    //[Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterModel model, 
        [FromServices] IMediator mediator,
        HttpContext httpContext)
    {
        // Handler
        try
        {
            await mediator.Send(new IdentityRegisterAccountCommand(model), httpContext.RequestAborted);
            return Results.Ok("User registered");
        }
        catch (Exception e)
        {
            return Results.InternalServerError();
        }

        //
        // var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        // var result = await userManager.CreateAsync(user, model.Password);
        // return result.Succeeded ? Results.Ok("User registered") : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginModel model, 
        [FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
        {
            return Results.Unauthorized();
        }

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        identity.AddClaim(ClaimTypes.NameIdentifier, user.Id);
        identity.AddClaim(ClaimTypes.Email, user.Email);

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.OfflineAccess);

        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> RefreshTokenAsync(HttpContext httpContext)
    {
        var request = httpContext.GetOpenIddictServerRequest();
        if (request == null || !request.IsRefreshTokenGrantType()) 
            return Results.BadRequest("Invalid request");

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        identity.AddClaim(ClaimTypes.NameIdentifier, "user-id");

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.OfflineAccess);

        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
