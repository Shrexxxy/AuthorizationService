using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Api.ServiceCollectionExtensions.OpenIdDict;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }
    public PermissionRequirement(string permissionName) => PermissionName = permissionName;
}