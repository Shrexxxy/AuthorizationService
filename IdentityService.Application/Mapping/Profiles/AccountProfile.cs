using AutoMapper;
using IdentityService.Application.Mapping.Converters;
using IdentityService.Application.Mapping.Converters.Account;
using IdentityService.Application.Model;
using Microsoft.AspNetCore.Identity;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityService.Application.Mapping.Profiles;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<RegisterModel, IdentityUser<Guid>>()
            .ConvertUsing<RegisterModelToIdentityUserConverter>();
    }
}