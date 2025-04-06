namespace IdentityService.Infrastructure.Exceptions;

/// <summary>
/// Исключение, указывающее на то, что указанное приложение не было найдено.
/// </summary>
public class ApplicationNotFoundException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ApplicationNotFoundException"/>.
    /// </summary>
    /// <param name="clientId">Идентификатор клиента, который не был найден.</param>
    public ApplicationNotFoundException(string clientId)
        : base($"Приложение с ClientId '{clientId}' не найдено.")
    {
    }
}
