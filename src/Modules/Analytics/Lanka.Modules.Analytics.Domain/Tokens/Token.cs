using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.IGAccounts;
using Lanka.Modules.Analytics.Domain.Tokens.AccessTokens;

namespace Lanka.Modules.Analytics.Domain.Tokens;

public class Token : Entity<TokenId>
{
    public BloggerId BloggerId { get; init; }

    public AccessToken AccessToken { get; init; }

    public DateTimeOffset LastCheckedOnUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    private Token() { }

    private Token(
        TokenId id,
        BloggerId bloggerId, 
        AccessToken accessToken,
        DateTimeOffset expiresAtUtc
    ) : base(id)
    {
        this.BloggerId = bloggerId;
        this.AccessToken = accessToken;
        this.ExpiresAtUtc = expiresAtUtc;
    }
    
    public static Result<Token> Create(
        Guid bloggerId,
        string accessToke,
        DateTimeOffset expiresAtUtc
    )
    {
        Result<AccessToken> validationResult = Validate(accessToke);
        
        if (validationResult.IsFailure)
        {
            return Result.Failure<Token>(validationResult.Error);
        }
        
        var token = new Token(
            TokenId.New(),
            new BloggerId(bloggerId),
            validationResult.Value,
            expiresAtUtc
        );
        
        return token;
    }
    
    private static Result<AccessToken> Validate(string accessToken)
    {
        Result<AccessToken> accessTokenResult = AccessToken.Create(accessToken);

        if (accessTokenResult.IsFailure)
        {
            return Result.Failure<AccessToken>(
                ValidationError.FromResults([accessTokenResult.Error])
            );
        }
        
        return Result<AccessToken>.Success(new AccessToken(accessToken));
    }
}
