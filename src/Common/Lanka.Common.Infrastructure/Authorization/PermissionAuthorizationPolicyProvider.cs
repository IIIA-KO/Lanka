using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Lanka.Common.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _permissionPolicies = new();

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        return this._permissionPolicies.GetOrAdd(
            policyName,
            static name => new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(name))
                .Build());
    }
}
