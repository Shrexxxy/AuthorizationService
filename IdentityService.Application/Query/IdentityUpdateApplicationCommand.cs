using IdentityService.Application.Model;
using MediatR;

namespace IdentityService.Application.Query;

/// <summary>
/// Команда для обновления данных приложения на основе заданного идентификатора клиента и модели обновления.
/// </summary>
/// <param name="TargetClientId">Идентификатор клиента приложения, которое нужно обновить.</param>
/// <param name="UpdateModel">Модель, содержащая данные для обновления приложения.</param>
public record UpdateApplicationCommand(string TargetClientId, ApplicationUpdateModel UpdateModel) : IRequest;
