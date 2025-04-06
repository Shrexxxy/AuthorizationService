using System.Text.Json;
using IdentityService.Application.Query.Application;
using IdentityService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityService.Application.Handlers.Application;

public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand>
{
    private readonly ILogger<UpdateApplicationCommandHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public UpdateApplicationCommandHandler(IOpenIddictApplicationManager applicationManager, ILogger<UpdateApplicationCommandHandler> logger)
    {
        _applicationManager = applicationManager;
        _logger = logger;
    }

    public async Task Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
    {
        // Найти приложение
        var applicationObj = await _applicationManager.FindByClientIdAsync(request.TargetClientId, cancellationToken);
        if (applicationObj is null)
        {
            throw new ApplicationNotFoundException(request.TargetClientId);
        }

        var application = (OpenIddictEntityFrameworkCoreApplication<Guid>)applicationObj;

        // Обновить RedirectUris
        if (request.UpdateModel.RedirectUris is null)
        {
            application.RedirectUris = null;
        }
        else
        {
            application.RedirectUris = JsonSerializer.Serialize<List<string>>(request.UpdateModel.RedirectUris);
        }

        // Обновить свойства приложения
        application.ClientId = request.UpdateModel.ClientId;
        application.DisplayName = request.UpdateModel.DisplayName;
        application.ConsentType = request.UpdateModel.ConsentType;
        application.ApplicationType = request.UpdateModel.Type;
        
        // Сохранить изменения
        await _applicationManager.UpdateAsync(application, cancellationToken);
        
        _logger.LogInformation($"Приложение успешно обновлено {application.DisplayName} | ClientId:{application.ClientId} | Тип:{application.ApplicationType} | Тип согласия:{application.ConsentType} | RedirectUris:{application.RedirectUris}");
    }
}