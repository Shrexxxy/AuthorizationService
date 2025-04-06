using AutoMapper;
using IdentityService.Application.Query.Application;
using IdentityService.Application.ViewModels;
using IdentityService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityService.Application.Handlers.Application;

/// <summary>
/// Обработчик запроса для получения информации о приложении по его ClientId.
/// </summary>
public class IdentityGetApplicationQueryHandler : IRequestHandler<IdentityGetApplicationQuery, ApplicationViewModel>
{
    private readonly ILogger<IdentityGetApplicationQueryHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="IdentityGetApplicationQueryHandler"/>.
    /// </summary>
    /// <param name="applicationManager">Менеджер приложений OpenIddict.</param>
    /// <param name="logger">Логгер для записи журналов сообщений в обработчике.</param>
    /// <param name="mapper">Сервис для маппинга объектов между моделями.</param>
    public IdentityGetApplicationQueryHandler(
        IOpenIddictApplicationManager applicationManager, 
        ILogger<IdentityGetApplicationQueryHandler> logger, 
        IMapper mapper)
    {
        _applicationManager = applicationManager;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Обрабатывает запрос на получение данных о приложении по его идентификатору клиента (ClientId).
    /// </summary>
    /// <param name="request">Объект запроса, содержащий идентификатор клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Возвращает объект <see cref="ApplicationViewModel"/> с данными о приложении.</returns>
    /// <exception cref="ApplicationNotFoundException">
    /// Выбрасывается, если приложение с указанным ClientId не найдено.
    /// </exception>
    /// <exception cref="Exception">
    /// Выбрасывается, если приложение не соответствует ожидаемому типу данных.
    /// </exception>
    public async Task<ApplicationViewModel> Handle(IdentityGetApplicationQuery request, CancellationToken cancellationToken)
    {
        // Пытаемся найти приложение по ClientId
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            // Приложение не найдено, выбрасываем исключение
            throw new ApplicationNotFoundException($"Приложение с идентификатором клиента {request.ClientId} не найдено.");
        }
        
        // Проверяем, соответствует ли тип приложения ожидаемому
        if (application is not OpenIddictEntityFrameworkCoreApplication<Guid>)
        {
            var message = $"Полученное приложение не соответствует ожидаемому типу ({typeof(OpenIddictEntityFrameworkCoreApplication<Guid>)}).";
            _logger.LogCritical(message);
            throw new Exception(message); //MicroserviceMappingException
        }

        // Маппим найденное приложение на объект ViewModel
        return _mapper.Map<ApplicationViewModel>((OpenIddictEntityFrameworkCoreApplication<Guid>)application);
    }
}