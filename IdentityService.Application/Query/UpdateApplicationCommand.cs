using IdentityService.Application.Model;
using MediatR;

namespace IdentityService.Application.Query;

public record UpdateApplicationCommand(string TargetClientId, ApplicationUpdateModel UpdateModel) : IRequest;