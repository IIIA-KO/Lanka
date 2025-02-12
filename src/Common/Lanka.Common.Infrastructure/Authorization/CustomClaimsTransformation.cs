using System.Security.Claims;
using Lanka.Common.Application.Authorization;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Common.Infrastructure.Authorization
{
    internal sealed class CustomClaimsTransformation(IServiceScopeFactory serviceScopeFactory) : IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.HasClaim(c => c.Type == CustomClaims.Sub))
            {
                return principal;
            }

            using IServiceScope scope = serviceScopeFactory.CreateScope();

            IPermissionService permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            string identityId = principal.GetIdentityId();

            Result<PermissionsResponse> result = await permissionService.GetUserPermissionsAsync(identityId);

            if (result.IsFailure)
            {
                throw new LankaException(nameof(IPermissionService.GetUserPermissionsAsync), result.Error);
            }

            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new Claim(CustomClaims.Sub, result.Value.UserId.ToString()));

            foreach (string permission in result.Value.Permissions)
            {
                claimsIdentity.AddClaim(new Claim(CustomClaims.Permission, permission));
            }

            principal.AddIdentity(claimsIdentity);

            return principal;
        }
    }
}
