using IdentityService.Application.Model;
using IdentityService.Application.Query;
using IdentityService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Endpoints;

public static class ApplicationsEndpoint
{
    public static void MapApplicationsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/applications")
            .WithOpenApi();

        api.MapPost("/create", CreateApplicationAsync)
            .WithDescription("Создает новое приложение на основе переданных данных.")
            //TODO: временно
            .AllowAnonymous();
        
        app.MapPut("/", UpdateApplication)
            .WithDescription("Обновляет приложение по client Id")
            //TODO: временно
            .AllowAnonymous();
        
        app.MapDelete("/", DeleteApplication)
            .WithDescription("Обновляет приложение по client Id")
            //TODO: временно
            .AllowAnonymous();
    }
    
    //TODO: временно
    //[Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    private static async Task<IResult> DeleteApplication(
        [FromQuery] string clientId,
        HttpContext httpContext,
        [FromServices] IMediator mediator)
    { 
        await mediator.Send(new DeleteApplicationCommand(clientId), httpContext.RequestAborted);
        return Results.Ok();
    }
    
    //TODO: временно
    //[Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    private static async Task<IResult> UpdateApplication(
        [FromQuery] string clientId,
        [FromBody] ApplicationUpdateModel model,
        [FromServices] IMediator mediator,
        HttpContext httpContext)
    {
        try
        { 
            await mediator.Send(new UpdateApplicationCommand(clientId, model), httpContext.RequestAborted);
            return Results.Ok();
        }
        catch (ApplicationNotFoundException e)
        {
            return Results.NotFound(e.Message);
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }

    }

    //TODO: временно
    //[Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    private static async Task<IResult> CreateApplicationAsync(
        [FromBody] CreateApplicationCommand command, 
        [FromServices] IMediator mediator)
    {
        try
        {
            await mediator.Send(command);
            return Results.Ok();
        }
        catch (InvalidOperationException e)
        {
            return Results.BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }
    }
}