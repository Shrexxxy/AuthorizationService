using MediatR;

namespace IdentityService.Application.Query;

/// <summary>
/// Команда для создания нового приложения с указанными параметрами,
/// такими как идентификатор клиента, секретный ключ, отображаемое имя и другие параметры конфигурации.
/// </summary>
public class IdentityCreateApplicationCommand : IRequest
{
    /// <summary>
    /// Уникальный идентификатор клиента.
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Секретный ключ клиента.
    /// </summary>
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// Отображаемое имя приложения.
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Тип согласия, связанный с приложением (опционально).
    /// </summary>
    public string? ConsentType { get; set; } = null!;

    /// <summary>
    /// Список разрешённых областей (scopes) для приложения (опционально).
    /// </summary>
    public List<string>? Scopes { get; set; }

    /// <summary>
    /// Список поддерживаемых типов авторизации (grant types) (опционально).
    /// </summary>
    public List<string>? GrandTypes { get; set; }

    /// <summary>
    /// Список разрешённых редирект-URL для приложения (опционально).
    /// </summary>
    public List<string>? RedirectUris { get; set; }
}