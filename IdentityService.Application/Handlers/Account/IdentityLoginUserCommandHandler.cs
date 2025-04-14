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

public class IdentityLoginUserCommandHandler : IRequestHandler<IdentityLoginUserCommand, IResult>
{
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

    public async Task<IResult> Handle(IdentityLoginUserCommand request, CancellationToken cancellationToken)
    {
        //
        var result = await AuthorizeCookieAsync();

        if (!result.Succeeded)
        {
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

        _user = await _userManager.GetUserAsync(result.Principal)
                ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        await ConfigureOpenIddictAsync();

        return await Authorize();

    }

    private async Task<AuthenticateResult> AuthorizeCookieAsync()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext);
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext.Request);
        }

        _httpContext = _httpContextAccessor.HttpContext;
        _openIddictRequest = _httpContextAccessor.HttpContext.GetOpenIddictServerRequest()
                             ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        return await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults
            .AuthenticationScheme);
    }

    private async Task ConfigureOpenIddictAsync()
    {
        ArgumentNullException.ThrowIfNull(_openIddictRequest);
        
        _openIddictApplication = await _openIddictApplicationManager.FindByClientIdAsync(_openIddictRequest.ClientId!, _httpContext.RequestAborted)
                                 ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        
        _openIddictApplicationId = await _openIddictApplicationManager.GetIdAsync(_openIddictApplication, _httpContext.RequestAborted)!
                                   ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

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

    public async Task<IResult> Authorize()
    {
        ArgumentNullException.ThrowIfNull(_user);

        ArgumentNullException.ThrowIfNull(_openIddictApplication);

        switch (await _openIddictApplicationManager.GetConsentTypeAsync(_openIddictApplication, _httpContext.RequestAborted))
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case OpenIddictConstants.ConsentTypes.External when _authorizations != null && !_authorizations.Any():
                return Results.Forbid(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }!));

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when _authorizations != null && _authorizations!.Any():
            case OpenIddictConstants.ConsentTypes.Explicit when _authorizations != null && _authorizations.Any() &&
                                                                !_openIddictRequest.HasPromptValue(OpenIddictConstants.PromptValues.Consent):
                return await IsExplictHasConsent();

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
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

            // In every other case, render the consent form.
            default:
                return Results.Challenge(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties { RedirectUri = "/" });
        }
    }
    
    private async Task<IResult> IsExplictHasConsent()
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(_user);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.

        principal.SetScopes(_openIddictRequest.GetScopes());
        principal.SetResources(await _openIddictScopeManager.ListResourcesAsync(principal.GetScopes(), _httpContext.RequestAborted).ToListAsync(_httpContext.RequestAborted));

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = _authorizations.LastOrDefault() 
                            ?? await _openIddictAuthorizationManager.CreateAsync(
                            principal: principal,
                            subject: _user.Id.ToString()!,
                            client: _openIddictApplicationId,
                            type: OpenIddictConstants.AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes(), 
                            cancellationToken: _httpContext.RequestAborted);

        //var identifier = await _openIddictApplicationManager.GetIdAsync(authorization);
        var identifier = await _openIddictAuthorizationManager.GetIdAsync(authorization, _httpContext.RequestAborted);
        principal.SetAuthorizationId(identifier);

        principal.SetDestinations(static claim => claim.Type switch
        {
            // If the "profile" scope was granted, allow the "name" claim to be
            // added to the access and identity tokens derived from the principal.
            OpenIddictConstants.Claims.Name when claim.Subject.HasScope(OpenIddictConstants.Scopes.Profile) => new[]
            {
                OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken
            },

            // Never add the "secret_value" claim to access or identity tokens.
            // In this case, it will only be added to authorization codes,
            // refresh tokens and user/device codes, that are always encrypted.
            "secret_value" => Array.Empty<string>(),

            // Otherwise, add the claim to the access tokens only.
            _ => new[] { OpenIddictConstants.Destinations.AccessToken }
        });



        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}