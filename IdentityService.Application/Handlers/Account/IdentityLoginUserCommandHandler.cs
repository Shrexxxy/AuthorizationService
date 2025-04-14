using System.Security.Claims;
using IdentityService.Application.Query.Account;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IdentityService.Application.Handlers.Account;

/// <summary>
/// Обработчик команды авторизации пользователя в Identity.
/// </summary>
public class IdentityLoginUserCommandHandler : IRequestHandler<IdentityLoginUserCommand, IResult>
{
    private const string SECRET_VALUE = "secret_value";
    
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOpenIddictApplicationManager _openIddictApplicationManager;
    private readonly IOpenIddictAuthorizationManager _openIddictAuthorizationManager;
    private readonly IOpenIddictScopeManager _openIddictScopeManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    
    private HttpContext _httpContext;
    private OpenIddictRequest _openIddictRequest;
    private IdentityUser<Guid> _user;
    private object _openIddictApplication;
    private List<object>? _authorizations;
    private string _openIddictApplicationId;

    /// <summary>
    /// Конструктор обработчика команды авторизации пользователя.
    /// </summary>
    public IdentityLoginUserCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IHttpContextAccessor httpContextAccessor,
        IOpenIddictApplicationManager openIddictApplicationManager,
        IOpenIddictAuthorizationManager openIddictAuthorizationManager,
        SignInManager<IdentityUser<Guid>> signInManager, 
        IOpenIddictScopeManager openIddictScopeManager)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _openIddictApplicationManager = openIddictApplicationManager;
        _openIddictAuthorizationManager = openIddictAuthorizationManager;
        _signInManager = signInManager;
        _openIddictScopeManager = openIddictScopeManager;
    }

    /// <summary>
    /// Хендлер авторизации
    /// </summary>
    public async Task<IResult> Handle(IdentityLoginUserCommand request, CancellationToken cancellationToken)
    {
        // Проверка авторизации на основе cookie.
        var result = await AuthorizeCookieAsync();

        if (!result.Succeeded)
        {
            // Если авторизация не удалась, выполняется перенаправление к страничке входа.
            ArgumentNullException.ThrowIfNull(_httpContext);
            return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = _httpContext.Request.PathBase + _httpContext.Request.Path + QueryString.Create(
                        _httpContext.Request.HasFormContentType
                            ? _httpContext.Request.Form.ToList()
                            : _httpContext.Request.Query.ToList())
                },
                new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }

        // Получение пользователя на основе Principal.
        _user = await _userManager.GetUserAsync(result.Principal)
                ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        // Настройка OpenIddict.
        await ConfigureOpenIddictAsync();

        // Выполнение процесса авторизации.
        return await Authorize();

    }

    /// <summary>
    /// Авторизация на основе cookie.
    /// </summary>
    /// <returns>Результат авторизации.</returns>
    private async Task<AuthenticateResult> AuthorizeCookieAsync()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext);
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext.Request);
        }

        _httpContext = _httpContextAccessor.HttpContext;
        
        // Получение OpenIddict-запроса.
        _openIddictRequest = _httpContextAccessor.HttpContext.GetOpenIddictServerRequest()
                             ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Выполнение аутентификации через cookie.
        return await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults
            .AuthenticationScheme);
    }

    /// <summary>
    /// Настройка клиента OpenIddict, проверка приложения и получение авторизаций.
    /// </summary>
    private async Task ConfigureOpenIddictAsync()
    {
        ArgumentNullException.ThrowIfNull(_openIddictRequest);
        
        // Поиск приложения клиента на основе идентификатора в запросе.
        _openIddictApplication = await _openIddictApplicationManager.FindByClientIdAsync(_openIddictRequest.ClientId!, _httpContext.RequestAborted)
                                 ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        
        _openIddictApplicationId = await _openIddictApplicationManager.GetIdAsync(_openIddictApplication, _httpContext.RequestAborted)!
                                   ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Проверка существующих авторизаций.
        _authorizations = (List<object>?)await _openIddictAuthorizationManager.FindAsync(
                                  subject: _user.Id.ToString(),
                                  client: _openIddictApplicationId,
                                  status: OpenIddictConstants.Statuses.Valid,
                                  type: OpenIddictConstants.AuthorizationTypes.Permanent,
                                  scopes: _openIddictRequest.GetScopes(),
                                  cancellationToken: _httpContext.RequestAborted)
                              .ToListAsync(_httpContext.RequestAborted)
                          ?? throw new InvalidOperationException("No authorization found");
    }

    /// <summary>
    /// Выполняет процесс авторизации в зависимости от типа согласия (consent).
    /// </summary>
    public async Task<IResult> Authorize()
    {
        ArgumentNullException.ThrowIfNull(_user);

        ArgumentNullException.ThrowIfNull(_openIddictApplication);

        switch (await _openIddictApplicationManager.GetConsentTypeAsync(_openIddictApplication, _httpContext.RequestAborted))
        {
            // Для внешнего согласия возвращается ошибка, если нет авторизаций.
            case OpenIddictConstants.ConsentTypes.External when _authorizations != null && !_authorizations.Any():
                return Results.Forbid(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }!));

            // Упрощенная авторизация в случае имплицитного согласия.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when _authorizations != null && _authorizations!.Any():
            case OpenIddictConstants.ConsentTypes.Explicit when _authorizations != null && _authorizations.Any() &&
                                                                !_openIddictRequest.HasPromptValue(OpenIddictConstants.PromptValues.Consent):
                return await IsExplictHasConsent();

            // Возврат ошибки, если требуется согласие пользователя для авторизации.
            case OpenIddictConstants.ConsentTypes.Explicit
                when _openIddictRequest.HasPromptValue(OpenIddictConstants.PromptValues.None):
            case OpenIddictConstants.ConsentTypes.Systematic
                when _openIddictRequest.HasPromptValue(OpenIddictConstants.PromptValues.None):
                return Results.Forbid(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }!));

            // Во всех остальных случаях пользователь будет перенаправлен на форму согласия.
            default:
                return Results.Challenge(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties { RedirectUri = "/" });
        }
    }
    
    /// <summary>
    /// Проверка наличия явного согласия пользователя.
    /// </summary>
    private async Task<IResult> IsExplictHasConsent()
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(_user);
        
        // Установка областей (scopes) для Principal.
        principal.SetScopes(_openIddictRequest.GetScopes());
        principal.SetResources(await _openIddictScopeManager.ListResourcesAsync(principal.GetScopes(), _httpContext.RequestAborted).ToListAsync(_httpContext.RequestAborted));

        // Создание постоянной авторизации.
        var authorization = _authorizations.LastOrDefault() 
                            ?? await _openIddictAuthorizationManager.CreateAsync(
                            principal: principal,
                            subject: _user.Id.ToString()!,
                            client: _openIddictApplicationId,
                            type: OpenIddictConstants.AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes(), 
                            cancellationToken: _httpContext.RequestAborted);
        
        var identifier = await _openIddictAuthorizationManager.GetIdAsync(authorization, _httpContext.RequestAborted);
        principal.SetAuthorizationId(identifier);

        // Установка claim-ов для токенов.
        principal.SetDestinations(static claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Name when claim.Subject.HasScope(OpenIddictConstants.Scopes.Profile) => new[]
            {
                OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken
            },
            SECRET_VALUE => Array.Empty<string>(),
            _ => new[] { OpenIddictConstants.Destinations.AccessToken }
        });


        // Возврат результата авторизации.
        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}