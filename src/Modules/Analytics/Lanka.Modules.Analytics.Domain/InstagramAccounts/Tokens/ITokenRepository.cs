namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

public interface ITokenRepository
{
    Task<Token?> GetByIdAsync(TokenId tokenId, CancellationToken cancellationToken = default);
    
    Task<Token?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    
    public void Add(Token token);
    
    public void Remove(Token token);
}
