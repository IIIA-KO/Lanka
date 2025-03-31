using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Application.Abstractions.Identity;

public interface IKeycloakAdminService
{
    Task<string> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default
    );

    Task<Result> TerminateUserSession(
        string userIdentityId,
        CancellationToken cancellationToken = default
    );

    Task<Result> DeleteAccountAsync(
        string userIdentityId,
        CancellationToken cancellationToken = default
    );

    Task<Result> LinkExternalIdentityProviderAccountToKeycloakUserAsync(
        string userIdentityId,
        string providerName,
        string providerUserId,
        string providerUsername,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsExternalIdentityProviderAccountLinkedAsync(
        string userIdentityId,
        string providerName,
        CancellationToken cancellationToken = default
    );

    Task<bool> CheckUserExistsInKeycloak(
        string email,
        CancellationToken cancellationToken = default
    );
}
