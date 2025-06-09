using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users.Emails;

namespace Lanka.Modules.Users.Application.Abstractions.Identity;

public interface IIdentityProviderService
{
    Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);

    Task<Result<AccessTokenResponse>> GetAccessTokenAsync(
        Email email,
        string password,
        CancellationToken cancellationToken = default
    );
    
    Task<Result<AccessTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    );
    
    Task<Result> TerminateUserSessionAsync(
        string userIdentityId,
        CancellationToken cancellationToken = default
    );
    
    Task<Result> DeleteUserAsync(
        string userIdentityId,
        CancellationToken cancellationToken = default
    );
    
    Task<Result> LinkExternalAccountToUserAsync(
        string userIdentityId,
        string providerName,
        string providerUserId,
        string providerUsername,
        CancellationToken cancellationToken = default
    );
    
    Task<bool> IsExternalAccountLinkedAsync(
        string userIdentityId,
        string providerName,
        CancellationToken cancellationToken = default
    );
    
    Task<bool> CheckUserExistsInKeycloak(
        string email,
        CancellationToken cancellationToken = default
    );
}
