using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Analytics.Infrastructure.InstagramAccounts;

internal sealed class InstagramAccountRepository : IInstagramAccountRepository
{
    private readonly AnalyticsDbContext _dbContext;

    public InstagramAccountRepository(AnalyticsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<InstagramAccount?> GetByIdAsync(
        InstagramAccountId instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await this._dbContext.InstagramAccounts
            .FirstOrDefaultAsync(igAccount => igAccount.Id == instagramAccountId, cancellationToken);
    }

    public async Task<InstagramAccount?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default
    )
    {
        return await this._dbContext.InstagramAccounts
            .FirstOrDefaultAsync(igAccount => igAccount.UserId == userId, cancellationToken);
    }

    public async Task<InstagramAccount?> GetByUserIdWithTokenAsync(
        UserId userId,
        CancellationToken cancellationToken = default
    )
    {
        return await this._dbContext.InstagramAccounts
            .Include(igAccount => igAccount.Token)
            .FirstOrDefaultAsync(igAccount => igAccount.UserId == userId, cancellationToken);
    }
    
    public async Task<InstagramAccount[]> GetOldAccountsAsync(
        int renewalThresholdInDays,
        int batchSize,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-renewalThresholdInDays);

        return await this._dbContext.InstagramAccounts
            .Where(igAccount =>
                igAccount.LastUpdatedAtUtc == null
                || igAccount.LastUpdatedAtUtc <= cutoff)
            .OrderBy(igAccount => igAccount.LastUpdatedAtUtc)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
    }

    public void Add(InstagramAccount instagramAccount)
    {
        this._dbContext.InstagramAccounts.Add(instagramAccount);
    }

    public void Remove(InstagramAccount instagramAccount)
    {
        this._dbContext.InstagramAccounts.Remove(instagramAccount);
    }
}
