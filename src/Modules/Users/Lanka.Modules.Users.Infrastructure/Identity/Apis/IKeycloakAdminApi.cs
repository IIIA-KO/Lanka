using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Refit;

namespace Lanka.Modules.Users.Infrastructure.Identity.Interfaces;

internal interface IKeycloakAdminApi
{
    [Post("/users")]
    Task<HttpResponseMessage> CreateUserAsync(
        [Body] UserRepresentation user
    );
    
    [Post("/users/{userId}/logout")]
    Task<HttpResponseMessage> LogoutUserAsync(
        [AliasAs("userId")] string userIdentityId
    );
    
    [Delete("/users/{userId}")]
    Task<HttpResponseMessage> DeleteUserAsync(
        [AliasAs("userId")] string userIdentityId
    );

    [Post("/users/{userId}/federated-identity/{providerName}")]
    Task<HttpResponseMessage> LinkProviderAsync(
        [AliasAs("userId")] string userIdentityId,
        [AliasAs("providerName")] string providerName,
        [Body] FederatedIdentity payload
    );

    [Get("/users/{userId}/federated-identity")]
    Task<List<FederatedIdentity>> GetFederatedIdentitiesAsync(
        [AliasAs("userId")] string userIdentityId
    );

    [Get("/users")]
    Task<List<UserRepresentation>> QueryUsersByEmailAsync([Query] string email);
}
