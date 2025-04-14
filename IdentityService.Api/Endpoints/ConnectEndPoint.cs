using IdentityService.Application.Model;
using IdentityService.Application.Query.Account;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Endpoints;

public static class ConnectEndPoint
{
    public static void MapConnectEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/connect");
        
        //default identity
        app.MapGet("~/Account/Login", (HttpContext httpContext) =>
        {
            return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = httpContext.Request.PathBase +
                                  httpContext.Request.Query.ToList().FirstOrDefault(x => x.Key == "ReturnUrl").Value
                },
                new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }).ExcludeFromDescription();
        
        api.MapPost("/authorize", Authorize)
            .WithOpenApi()
            .WithDescription("Авторизация пользователя.")
            .AllowAnonymous();
    }
    
    private static async Task<IResult> Authorize(
        [FromBody] LoginModel model, 
        [FromServices] IMediator mediator,
        HttpContext httpContext)
    {
        try
        {
            await mediator.Send(new IdentityLoginUserCommand(model), httpContext.RequestAborted);
            return Results.Ok("User authorize");
        }
        catch (Exception e)
        {
            return Results.InternalServerError();
        }
    }
}