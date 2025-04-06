using AuthorizationService.Application.Model;
using MediatR;

namespace AuthorizationService.Application.Query;

public class UpdateApplicationCommand : IRequest
{
    public string TargetClientId { get; set; } = null!;
    public ApplicationUpdateModel UpdateModel { get; set; } = null!;

    public UpdateApplicationCommand(string targetClientId, ApplicationUpdateModel updateModel)
    {
        TargetClientId = targetClientId;
        UpdateModel = updateModel;
    }
}