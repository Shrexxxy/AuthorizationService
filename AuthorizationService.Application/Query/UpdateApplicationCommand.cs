using AuthorizationService.Application.Model;
using MediatR;

namespace AuthorizationService.Application.Query;

public record UpdateApplicationCommand(string TargetClientId, ApplicationUpdateModel UpdateModel) : IRequest;