using Lanka.Modules.Analytics.Application.UnitTests.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

namespace Lanka.Modules.Analytics.Application.UnitTests.Tokens;

internal static class TokenData
{
    public static Token Create()
    {
        return Token.Create(
            UserId,
            AccessToken,
            ExpiresAtUtc,
            new InstagramAccountId(Guid.NewGuid())
        ).Value;
    }

    public static InstagramAccount InstagramAccount => InstagramAccountData.Create();
    
    public static Guid UserId => Guid.NewGuid();

    public static string AccessToken => "valid_access_token_1234567890";

    public static DateTimeOffset ExpiresAtUtc => DateTimeOffset.UtcNow.AddDays(30);

    public static Guid InstagramAccountId => Guid.NewGuid();
}
