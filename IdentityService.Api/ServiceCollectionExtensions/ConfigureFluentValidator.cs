using FluentValidation;
using IdentityService.Application.Validators;

namespace IdentityService.Api.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureFluentValidator(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddValidatorsFromAssembly(typeof(DiValidatorDetector).Assembly);
        services.AddValidatorsFromAssemblyContaining<DiValidatorDetector>();
        return services;
    }
}
