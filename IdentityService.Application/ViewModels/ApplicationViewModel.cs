namespace IdentityService.Application.ViewModels;

public class ApplicationViewModel
{
    /// <summary>
    /// Уникальный идентификатор, связанный с текущим приложением.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор клиента, связанный с текущим приложением.
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Секрет клиента, связанный с текущим приложением.
    /// Примечание: в зависимости от используемого менеджера приложений для создания этого экземпляра
    /// это свойство может быть захешировано или зашифровано в целях безопасности.
    /// </summary>
    public string? ClientSecret { get; set; }
    
    /// <summary>
    /// Тип согласия, связанный с текущим приложением.
    /// </summary>
    public string? ConsentType { get; set; }
    
    /// <summary>
    /// Отображаемое имя, связанное с текущим приложением.
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Указывает или задает разрешения, связанные с текущим приложением, сериализованные в виде JSON-массива.
    /// </summary>
    public PermissionsViewModel? Permissions { get; set; }
    
    /// <summary>
    /// URI перенаправления, связанные с текущим приложением, сериализованные в виде JSON-массива.
    /// </summary>
    public List<string>? RedirectUris { get; set; }
    
    /// <summary>
    /// Тип приложения, связанный с текущим приложением.
    /// </summary>
    public string? Type { get; set; }
}