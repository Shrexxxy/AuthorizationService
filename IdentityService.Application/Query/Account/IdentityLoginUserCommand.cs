using IdentityService.Application.Model;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Application.Query.Account;

public record IdentityLoginUserCommand() : IRequest<IResult>;