using Lanka.Common.Application.Authorization;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Users.GetUserPermissions;
using MediatR;

namespace Lanka.Modules.Users.Infrastructure.Authorization;

public class PermissionService : IPermissionService
{
    private readonly ISender _sender;

    public PermissionService(ISender sender)
    {
        this._sender = sender;
    }

    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        return await this._sender.Send(new GetUserPermissionsQuery(identityId));
    }
}