using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.Tokens.AccessTokens;

public static class AccessTokenErrors
{
    public static Error Empty => Error.Failure(
        "AccessToken.Empty",
        "Access token cannot be empty."
    );
}
