using MediatR;

namespace AuthorizationService.Application.Query;

public record DeleteApplicationCommand(string ClientId) : IRequest;