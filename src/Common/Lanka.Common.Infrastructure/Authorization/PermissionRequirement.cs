using Microsoft.AspNetCore.Authorization;

namespace Lanka.Common.Infrastructure.Authorization;

internal sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        this.Permission = permission;
    }

    public string Permission { get; }
}