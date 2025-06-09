using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;
using Lanka.Modules.Analytics.Infrastructure.Database;

namespace Lanka.Modules.Analytics.Infrastructure.Tokens;

internal sealed class TokenRepository : ITokenRepository
{
    private readonly AnalyticsDbContext _dbContext;

    public TokenRepository(AnalyticsDbContext dbContext)
    {
        this._dbContext = dbContext;
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
