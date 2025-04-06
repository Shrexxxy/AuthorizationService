using IdentityService.Application.Query.Application;
using IdentityService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace IdentityService.Application.Handlers.Application;

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