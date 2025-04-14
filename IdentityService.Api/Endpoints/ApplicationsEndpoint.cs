using System.Net;
using AuthorizationService.DAL;
using IdentityService.Application.Model;
using IdentityService.Application.Query;
using IdentityService.Application.Query.Application;
using IdentityService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Endpoints;

public static class ApplicationsEndpoint
{
    public static void MapApplicationsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/superadmin/applications")
            .WithOpenApi();


        api.MapGet("/", GetApplication).WithDescription("Получает данные приложения по client Id.");
        api.MapPost("/", CreateApplication).WithDescription("Создает новое приложение на основе переданных данных.");
        api.MapPut("/", UpdateApplication).WithDescription("Обновляет приложение по client Id");
        api.MapDelete("/", DeleteApplication).WithDescription("Удаляет приложение по client Id");
    }
    
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType(statusCode: (int)HttpStatusCode.OK, type: typeof(OkResult))]
    [ProducesResponseType(statusCode: (int)HttpStatusCode.BadRequest, type: typeof(OkResult))]
    [ProducesResponseType(statusCode: (int)HttpStatusCode.NotFound, type: typeof(OkResult))]
    [ProducesResponseType(statusCode: (int)HttpStatusCode.InternalServerError, type: typeof(OkResult))]
    private static async Task<IResult> GetApplication(
        [FromQuery] string clientId,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        HttpContext httpContext
    )
    {
        var result = await mediator.Send(new IdentityGetApplicationQuery(clientId), httpContext.RequestAborted);
        return Results.Ok(result);
    }
    
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    private static async Task<IResult> DeleteApplication(
        [FromQuery] string clientId,
        HttpContext httpContext,
        [FromServices] IMediator mediator)
    { 
        await mediator.Send(new DeleteApplicationCommand(clientId), httpContext.RequestAborted);
        return Results.Ok();
    }
    
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
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
    
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.SuperAdmin)]
    private static async Task<IResult> CreateApplication(
        [FromBody] IdentityCreateApplicationCommand command, 
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