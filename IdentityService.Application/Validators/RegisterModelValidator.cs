using FluentValidation;
using IdentityService.Application.Model;

namespace IdentityService.Application.Validators;

/// <summary>
/// Валидатор для модели регистрации пользователя.
/// Проверяет обязательность всех полей, минимальные/максимальные длины и корректность формата данных.
/// </summary>
public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    /// <summary>
    /// Конструктор валидатора для модели регистрации.
    /// Устанавливает правила для проверки данных регистрации.
    /// </summary>
    public RegisterModelValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно для заполнения.")
            .MinimumLength(2).WithMessage("Имя пользователя должно содержать не менее 2 символов.")
            .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен для заполнения.")
            .EmailAddress().WithMessage("Email должен быть в правильном формате.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен для заполнения.")
            .Matches(@"^\+?\d+$").WithMessage("Номер телефона должен быть в правильном формате (только цифры, допускается '+').");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения.")
            .MinimumLength(6).WithMessage("Пароль должен содержать не менее 6 символов.")
            .MaximumLength(100).WithMessage("Пароль не должен превышать 100 символов.");

    }
}