using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Analytics.Infrastructure.Tokens;

internal sealed class TokenRepository : ITokenRepository
{
    private readonly AnalyticsDbContext _dbContext;

    public TokenRepository(AnalyticsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Token?> GetByIdAsync(TokenId tokenId, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Tokens.FirstOrDefaultAsync(token => token.Id == tokenId, cancellationToken);
    }

    public async Task<Token?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Tokens.FirstOrDefaultAsync(token => token.UserId == userId, cancellationToken);
    }

    public void Add(Token token)
    {
        this._dbContext.Tokens.Add(token);
    }

    public void Remove(Token token)
    {
        this._dbContext.Tokens.Remove(token);
    }
}
