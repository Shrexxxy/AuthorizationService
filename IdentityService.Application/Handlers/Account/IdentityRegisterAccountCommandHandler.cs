using System.Text;
using AuthorizationService.DAL;
using AutoMapper;
using FluentValidation;
using IdentityService.Application.Model;
using IdentityService.Application.Query.Account;
using IdentityService.Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace IdentityService.Application.Handlers.Account;

/// <summary>
/// Обработчик команды регистрации аккаунта. 
/// Предназначен для выполнения логики создания нового пользователя, проверки данных и регистрации.
/// </summary>
public class IdentityRegisterAccountCommandHandler : IRequestHandler<IdentityRegisterAccountCommand>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<IdentityRegisterAccountCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterModel> _registerModelValidator;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly UserClaimsPrincipalFactory<IdentityUser<Guid>, IdentityRole<Guid>> _userClaims;

    /// <summary>
    /// Конструктор обработчика IdentityRegisterAccountCommandHandler
    /// </summary>
    /// <param name="mapper">Интерфейс для маппинга объектов между моделями</param>
    /// <param name="registerModelValidator">Валидатор для проверки данных модели регистрации</param>
    /// <param name="logger">Логгер для записи сообщений</param>
    /// <param name="userManager">Менеджер пользователей для управления учетными записями</param>
    /// <param name="dbContext">Контекст базы данных приложения</param>
    /// <param name="userClaims">Фабрика для создания ClaimsPrincipal пользователя</param>
    public IdentityRegisterAccountCommandHandler(
        IMapper mapper,
        IValidator<RegisterModel> registerModelValidator,
        ILogger<IdentityRegisterAccountCommandHandler> logger,
        AppDbContext dbContext, 
        UserManager<IdentityUser<Guid>> userManager, 
        UserClaimsPrincipalFactory<IdentityUser<Guid>, IdentityRole<Guid>> userClaims)
    {
        _mapper = mapper;
        _registerModelValidator = registerModelValidator;
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
        _userClaims = userClaims;
    }

    /// <summary>
    /// Основной метод обработки команды регистрации аккаунта.
    /// </summary>
    /// <param name="request">Запрос, содержащий данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены для асинхронного выполнения</param>
    public async Task Handle(IdentityRegisterAccountCommand request, CancellationToken cancellationToken = default)
    {
        // Логируем начало регистрации
        _logger.LogInformation("Начало обработки команды регистрации аккаунта.");

        var validationResult = await _registerModelValidator.ValidateAsync(request.RegisterModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            // Логирование ошибки при невалидных данных
            _logger.LogWarning("Ошибка валидации данных регистрации: {Ошибки}", string.Join(",", validationResult.Errors));
            throw new ValidationException(validationResult.Errors.ToString());

        }

        // Создаем аккаунт пользователя
        await CreateAccountAsync(request.RegisterModel, cancellationToken);
        _logger.LogInformation("Аккаунт успешно создан для пользователя: {UserName}", request.RegisterModel.UserName);
    }

    /// <summary>
    /// Метод для создания нового аккаунта пользователя на основе модели регистрации.
    /// </summary>
    /// <param name="model">Модель, содержащая данные для регистрации.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронного выполнения.</param>
    private async Task CreateAccountAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        // Логируем начало создания нового аккаунта
        _logger.LogInformation("Начало создания нового аккаунта пользователя...");

        // Маппинг модели регистрации в объект IdentityUser
        var user = _mapper.Map<IdentityUser<Guid>>(model);

        // Начинаем транзакцию для обеспечения атомарности операции
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Проверяем, существует ли пользователь с таким же email
        var existUserEmail = await _userManager.FindByEmailAsync(user.Email);
        if (existUserEmail is not null)
        {
            _logger.LogWarning("Регистрация отменена: пользователь с email {Email} уже существует", user.Email);
            throw new EmailAlreadyExistsException(user.Email);
        }

        // Проверяем, существует ли пользователь с таким же логином
        var existUserUserName = await _userManager.FindByNameAsync(user.UserName);
        if (existUserUserName is not null)
        {
            _logger.LogWarning("Регистрация отменена: пользователь с логином {Login} уже существует", user.UserName);
            throw new LoginAlreadyExistsException(user.UserName);
        }

        // Проверяем, существует ли пользователь с таким же номером телефона
        var existUserPhone = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == user.PhoneNumber, cancellationToken);
        if (existUserPhone is not null)
        {
            _logger.LogWarning("Регистрация отменена: пользователь с номером телефона {Phone} уже существует",
                user.PhoneNumber);
            throw new PhoneAlreadyExistsException(user.PhoneNumber);
        }

        // Пытаемся создать пользователя
        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            // Если пользователь не был создан, откатываем транзакцию
            await _dbContext.Database.RollbackTransactionAsync(cancellationToken);

            // Формируем список ошибок
            var errors = createResult.Errors.Select(x => $"{x.Code}: {x.Description}").ToList();
            _logger.LogWarning(
                "Ошибка регистрации пользователя: email:{Email} | ошибки: {Errors}", model.Email,
                string.Join(", ", errors));

            // Выбрасываем исключение о проваленной регистрации
            throw new InvalidOperationException(
                $"Ошибка регистрации пользователя: email:{model.Email} | ошибки: {string.Join(", ", errors)}");
        }

        // Назначаем только что созданному пользователю базовую роль "User"
        var addRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.User);
        if (!addRoleResult.Succeeded)
        {
            // Если не удалось добавить роль, откатываем транзакцию
            await _dbContext.Database.RollbackTransactionAsync(cancellationToken);

            // Формируем список ошибок для роли
            var errorsRole = addRoleResult.Errors
                .Select(x => $"{x.Code}: {x.Description}")
                .ToList();

            // Логируем и кидаем исключение
            _logger.LogWarning("Ошибка при добавлении роли пользователю: {Errors}", string.Join(", ", errorsRole));
            throw new InvalidOperationException(string.Join(",\n", errorsRole));
        }

        // Сохраняем изменения в базе данных
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Генерируем ClaimsPrincipal для пользователя (нужно для аутентификации)
        var principal = await _userClaims.CreateAsync(user);

        // Коммитим транзакцию, так как все прошло успешно
        await _dbContext.Database.CommitTransactionAsync(cancellationToken);

        // Логируем успешную регистрацию
        _logger.LogInformation("Пользователь с email: {UserName} успешно зарегистрирован", user.UserName);

        // TODO: Возможно, позже нужно вернуть какие-либо данные
        // return mappedUser;
    }

}