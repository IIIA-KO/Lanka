using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users.Emails;

namespace Lanka.Modules.Users.Application.Abstractions.Identity;

public interface IKeycloakTokenService
{
    Task<Result<AccessTokenResponse>> GetAccessTokenAsync(
        Email email,
        string password,
        CancellationToken cancellationToken = default
    );

    Task<Result<AccessTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    );
}
