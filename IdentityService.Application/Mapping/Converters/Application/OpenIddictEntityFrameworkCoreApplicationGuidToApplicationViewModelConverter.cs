using System.Text.Json;
using AutoMapper;
using IdentityService.Application.ViewModels;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityService.Application.Mapping.Converters.Application;

public class OpenIddictEntityFrameworkCoreApplicationGuidToApplicationViewModelConverter : ITypeConverter<OpenIddictEntityFrameworkCoreApplication<Guid>, ApplicationViewModel>
{
    public ApplicationViewModel Convert(OpenIddictEntityFrameworkCoreApplication<Guid> source, ApplicationViewModel? destination, ResolutionContext context)
    {
        if (destination is null)
        {
            destination = new ApplicationViewModel();
        }

        destination.Id = source.Id;
        destination.ClientId = source.ClientId;
        destination.DisplayName = source.DisplayName;
        destination.ClientSecret = source.ClientSecret;
        destination.Type = source.ApplicationType;
        destination.ConsentType = source.ConsentType;

        if (source.RedirectUris != null)
            destination.RedirectUris = JsonSerializer.Deserialize<List<string>>(source.RedirectUris);
        
        if (source.Permissions != null)
        {
            var permissions = JsonSerializer.Deserialize<List<string>>(source.Permissions);
            destination.Permissions = new PermissionsViewModel();
            foreach (var permission in permissions!)
            {
                var prefix = permission.Split(":").FirstOrDefault()+":";
                switch (prefix)
                {
                    case OpenIddictConstants.Permissions.Prefixes.Scope:
                        destination.Permissions.Scopes.Add(permission.Split(prefix)[1]);
                        break;
                    case OpenIddictConstants.Permissions.Prefixes.Endpoint:
                        destination.Permissions.Endpoints.Add(permission.Split(prefix)[1]);
                        break;
                    case OpenIddictConstants.Permissions.Prefixes.GrantType:
                        destination.Permissions.GrantTypes.Add(permission.Split(prefix)[1]);
                        break;
                    case OpenIddictConstants.Permissions.Prefixes.ResponseType:
                        destination.Permissions.ResponseTypes.Add(permission.Split(prefix)[1]);
                        break;
                }
            }
        }

        return destination;
    }
}