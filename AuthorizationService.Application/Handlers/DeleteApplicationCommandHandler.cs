using System.Net;
using AuthorizationService.Application.Query;
using AuthorizationService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace AuthorizationService.Application.Handlers;

public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand>
{
    private readonly ILogger<DeleteApplicationCommandHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public DeleteApplicationCommandHandler(ILogger<DeleteApplicationCommandHandler> logger, IOpenIddictApplicationManager applicationManager)
    {
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public async Task Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            throw new ApplicationNotFoundException(request.ClientId);
        }
        
        await _applicationManager.DeleteAsync(application, cancellationToken);
        _logger.LogInformation($"Application by id {request.ClientId} is successfully deleted");
    }
}