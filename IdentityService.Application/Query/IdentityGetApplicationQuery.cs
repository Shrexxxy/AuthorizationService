using IdentityService.Application.ViewModels;
using MediatR;

namespace IdentityService.Application.Query;

/// <summary>
/// Запрос для получения информации о приложении на основе его идентификатора клиента (ClientId).
/// </summary>
/// <param name="ClientId">Идентификатор клиента приложения, информацию о котором нужно получить.</param>
public record IdentityGetApplicationQuery(string ClientId) : IRequest<ApplicationViewModel>;
