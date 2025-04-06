using MediatR;

namespace IdentityService.Application.Query;

/// <summary>
/// Команда для удаления приложения на основе его идентификатора клиента (ClientId).
/// </summary>
/// <param name="ClientId">Идентификатор клиента приложения, которое нужно удалить.</param>
public record DeleteApplicationCommand(string ClientId) : IRequest;
