using IdentityService.Infrastructure.Extensions;
using IdentityService.Application.Query;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace IdentityService.Application.Handlers;

/// <summary>
/// Обработчик команды <see cref="IdentityCreateApplicationCommand"/> для создания нового приложения.
/// Использует OpenIddict для управления приложениями.
/// </summary>
public class CreateApplicationCommandHandler : IRequestHandler<IdentityCreateApplicationCommand>
{
    private readonly ILogger<CreateApplicationCommandHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CreateApplicationCommandHandler"/>.
    /// </summary>
    /// <param name="applicationManager">Менеджер OpenIddict для управления приложениями.</param>
    /// <param name="logger">Логгер для записи диагностической информации.</param>
    public CreateApplicationCommandHandler(
        IOpenIddictApplicationManager applicationManager,
        ILogger<CreateApplicationCommandHandler> logger)
    {
        _applicationManager = applicationManager;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатывает команду <see cref="IdentityCreateApplicationCommand"/> и создает новое приложение.
    /// </summary>
    /// <param name="request">Запрос, содержащий информацию о приложении для создания.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Асинхронная задача.</returns>
    public async Task Handle(IdentityCreateApplicationCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, существует ли уже приложение с заданным ClientId.
        var existingApp = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (existingApp != null)
        {
            // Если приложение с таким ClientId уже существует, выбрасываем исключение.
            throw new InvalidOperationException("Application already exists");
        }

        // Создаем объект для описания нового приложения.
        var client = new OpenIddictApplicationDescriptor
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            DisplayName = request.DisplayName,
            ConsentType = request.ConsentType,
        };

        // Добавляем разрешенные области (scopes) к приложению.
        client.AddScopes(request.Scopes);

        // Добавляем поддерживаемые типы авторизации (grant types).
        client.AddGrandTypes(request.GrandTypes);

        // Добавляем разрешенные URI перенаправления (redirect URIs).
        client.AddRedirectUris(request.RedirectUris);

        // Добавляем разрешенные типы ответов.
        client.AddResponseTypes();

        // Добавляем конечные точки (endpoints), которые могут использоваться приложением.
        client.AddEndpoints();

        // Пытаемся создать новое приложение через менеджер.
        await _applicationManager.CreateAsync(client, cancellationToken);

        // Логируем информацию об успешном создании приложения.
        _logger.LogInformation($"Приложение с DisplayName: {request.DisplayName} и ClientId: {request.ClientId} успешно создано");

    }
}