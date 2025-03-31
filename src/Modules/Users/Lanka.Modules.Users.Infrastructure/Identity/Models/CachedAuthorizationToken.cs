namespace Lanka.Modules.Users.Infrastructure.Identity.Models;

internal sealed record CachedAuthorizationToken(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    int RefreshExpiresIn,
    DateTimeOffset AcquiredAt
);
