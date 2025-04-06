namespace IdentityService.Infrastructure.Exceptions;

public class PhoneAlreadyExistsException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="PhoneAlreadyExistsException"/>.
    /// </summary>
    /// <param name="phone">Номер телефона, который уже существует</param>
    public PhoneAlreadyExistsException(string phone)
        : base($"Аккаунт с логином '{phone}' уже зарегистрирован.")
    {
    }
}
