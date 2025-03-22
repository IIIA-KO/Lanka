using Lanka.Modules.Campaigns.Domain.BlockedDates;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.BlockedDates;

internal sealed class BlockedDateRepository : IBlockedDateRepository
{
    private readonly CampaignsDbContext _dbContext;

    public BlockedDateRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<BlockedDate?> GetByIdAsync(
        BlockedDateId id, 
        CancellationToken cancellationToken = default
    )
    {
        return await this._dbContext.BlockedDates.FirstOrDefaultAsync(
            bd => bd.Id == id,
            cancellationToken
        );
    }

    public async Task<BlockedDate?> GetByDateAndBloggerIdAsync(
        DateOnly date, 
        BloggerId bloggerId,
        CancellationToken cancellationToken = default
    )
    {
        return await this._dbContext.BlockedDates.FirstOrDefaultAsync(
            bd => bd.Date == date && bd.BloggerId == bloggerId,
            cancellationToken
        );
    }

    public void Add(BlockedDate blockedDate)
    {
        this._dbContext.BlockedDates.Add(blockedDate);
    }

    public void Remove(BlockedDate blockedDate)
    {
        this._dbContext.BlockedDates.Remove(blockedDate);
    }
}
