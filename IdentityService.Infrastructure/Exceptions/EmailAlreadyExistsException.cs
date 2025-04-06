namespace IdentityService.Infrastructure.Exceptions;

public class EmailAlreadyExistsException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="EmailAlreadyExistsException"/>.
    /// </summary>
    /// <param name="email">Почта, которая уже существует</param>
    public EmailAlreadyExistsException(string email)
        : base($"Аккаунт с почтой '{email}' уже зарегистрирован.")
    {
    }
}
