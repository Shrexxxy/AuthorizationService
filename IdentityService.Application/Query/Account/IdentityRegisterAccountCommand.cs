using IdentityService.Application.Model;
using MediatR;

namespace IdentityService.Application.Query.Account;

public record IdentityRegisterAccountCommand(RegisterModel RegisterModel) : IRequest;