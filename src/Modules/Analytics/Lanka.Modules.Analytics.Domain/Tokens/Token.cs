using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Tokens.AccessTokens;

namespace Lanka.Modules.Analytics.Domain.Tokens;

public class Token : Entity<TokenId>
{
    public UserId UserId { get; init; }

    public AccessToken AccessToken { get; private set; }

    public DateTimeOffset LastCheckedOnUtc { get; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public InstagramAccountId InstagramAccountId { get; init; }

    public InstagramAccount InstagramAccount { get; init; }

    private Token() { }

    private Token(
        TokenId id,
        UserId userId,
        AccessToken accessToken,
        DateTimeOffset expiresAtUtc,
        InstagramAccountId instagramAccountId
    ) : base(id)
    {
        this.UserId = userId;
        this.AccessToken = accessToken;
        this.ExpiresAtUtc = expiresAtUtc;
        this.InstagramAccountId = instagramAccountId;
    }

    public static Result<Token> Create(
        Guid userId,
        string accessToken,
        DateTimeOffset expiresAtUtc,
        InstagramAccountId instagramAccountId
    )
    {
        Result<AccessToken> validationResult = Validate(accessToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Token>(validationResult.Error);
        }

        var token = new Token(
            TokenId.New(),
            new UserId(userId),
            validationResult.Value,
            expiresAtUtc,
            instagramAccountId
        );

        return token;
    }

    public Result Update(string accessToken, DateTimeOffset expiresAtUtc)
    {
        Result<AccessToken> validationResult = Validate(accessToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Token>(validationResult.Error);
        }

        this.AccessToken = validationResult.Value;
        this.ExpiresAtUtc = expiresAtUtc;
        
        return Result.Success();
    }

    private static Result<AccessToken> Validate(string accessToken)
    {
        Result<AccessToken> accessTokenResult = AccessToken.Create(accessToken);

        return new ValidationBuilder()
            .Add(accessTokenResult)
            .Build(() => accessTokenResult.Value);
    }
}
