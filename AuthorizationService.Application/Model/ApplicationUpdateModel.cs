namespace AuthorizationService.Application.Model;

/// <summary>
/// Модель для обновления данных приложения.
/// </summary>
public class ApplicationUpdateModel
{
    /// <summary>
    /// Уникальный идентификатор клиента.
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Тип согласия для приложения.
    /// </summary>
    public string? ConsentType { get; set; }
    
    /// <summary>
    /// Отображаемое имя приложения.
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Список перенаправлений URI для приложения.
    /// </summary>
    public List<string>? RedirectUris { get; set; }
    
    /// <summary>
    /// Тип приложения.
    /// </summary>
    /// <example>
    /// конфиденциальное или общедоступное
    /// </example>
    public string? Type { get; set; }

}