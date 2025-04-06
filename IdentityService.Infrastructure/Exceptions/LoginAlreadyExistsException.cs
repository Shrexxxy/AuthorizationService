namespace IdentityService.Infrastructure.Exceptions;

public class LoginAlreadyExistsException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="LoginAlreadyExistsException"/>.
    /// </summary>
    /// <param name="userName">Логин, который уже существует</param>
    public LoginAlreadyExistsException(string userName)
        : base($"Аккаунт с логином '{userName}' уже зарегистрирован.")
    {
    }
}
