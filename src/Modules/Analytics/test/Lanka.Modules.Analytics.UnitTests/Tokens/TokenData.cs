namespace Lanka.Modules.Analytics.UnitTests.Tokens;

internal static class TokenData
{
    public static Guid UserId => Guid.NewGuid();
    
    public static string AccessToken => "valid_access_token_1234567890";
    
    public static DateTimeOffset ExpiresAtUtc => DateTimeOffset.UtcNow.AddDays(30);
    
    public static Guid InstagramAccountId => Guid.NewGuid();
    
}
