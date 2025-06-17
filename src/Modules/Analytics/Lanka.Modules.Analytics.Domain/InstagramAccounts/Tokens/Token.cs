using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens.AccessTokens;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

public class Token : Entity<TokenId>
{
    public UserId UserId { get; init; }

    public AccessToken AccessToken { get; init; }

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
        string accessToke,
        DateTimeOffset expiresAtUtc,
        InstagramAccountId instagramAccountId
    )
    {
        Result<AccessToken> validationResult = Validate(accessToke);
        
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
    
    private static Result<AccessToken> Validate(string accessToken)
    {
        Result<AccessToken> accessTokenResult = AccessToken.Create(accessToken);

        if (accessTokenResult.IsFailure)
        {
            return Result.Failure<AccessToken>(
                ValidationError.FromResults([accessTokenResult])
            );
        }
        
        return AccessToken.Create(accessToken).Value;
    }
}
