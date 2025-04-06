using MediatR;

namespace IdentityService.Application.Query;

public record DeleteApplicationCommand(string ClientId) : IRequest;