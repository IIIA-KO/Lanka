using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.Tokens.AccessTokens;

public sealed record AccessToken
{
    public string Value { get; init; }

    private AccessToken(string value)
    {
        this.Value = value;
    }

    public static Result<AccessToken> Create(string accessToken)
    {
        Result validationResult = ValidateAccessTokenString(accessToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<AccessToken>(validationResult.Error);
        }

        return new AccessToken(accessToken);
    }

    private static Result ValidateAccessTokenString(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return AccessTokenErrors.Empty;
        }
        
        return Result.Success();
    }
}
