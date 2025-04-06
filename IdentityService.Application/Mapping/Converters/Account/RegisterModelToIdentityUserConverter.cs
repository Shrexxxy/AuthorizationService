using AutoMapper;
using IdentityService.Application.Model;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Mapping.Converters.Account;

public class RegisterModelToIdentityUserConverter : ITypeConverter<RegisterModel, IdentityUser<Guid>>
{
    public IdentityUser<Guid> Convert(RegisterModel source, IdentityUser<Guid> destination, ResolutionContext context)
    {
        destination ??= new();
        destination.UserName = source.UserName;
        destination.Email = source.Email;
        destination.PhoneNumber = source.PhoneNumber;
        destination.EmailConfirmed = source.EmailConfirmed;
        destination.PhoneNumberConfirmed = source.PhoneNumberConfirmed;

        return destination;
    }
}