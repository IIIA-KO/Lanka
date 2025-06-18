using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

public static class TokenErrors
{
    public static Error NotFound => Error.NotFound(
        "InstagramToken.NotFound",
        "Token with specified identifier was not found"
    );
}
