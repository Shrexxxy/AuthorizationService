using AutoMapper;
using IdentityService.Application.Mapping.Converters;
using IdentityService.Application.Mapping.Converters.Application;
using IdentityService.Application.ViewModels;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityService.Application.Mapping.Profiles;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<OpenIddictEntityFrameworkCoreApplication<Guid>, ApplicationViewModel>()
            .ConvertUsing<OpenIddictEntityFrameworkCoreApplicationGuidToApplicationViewModelConverter>();
    }
}