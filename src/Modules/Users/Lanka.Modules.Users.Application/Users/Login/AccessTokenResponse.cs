namespace Lanka.Modules.Users.Application.Users.Login;

public sealed record AccessTokenResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    int RefreshExpiresIn
);
