using AuthorizationService.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationService.Api.Endpoints;

public static class ApplicationsEndpoint
{
    public static void MapApplicationsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/applications");

        api.MapPost("/create", CreateApplicationAsync)
            .WithOpenApi()
            .WithDescription("Создает новое приложение на основе переданных данных.")
            .AllowAnonymous();
    }

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