using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Reviews;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Reviews;

public class ReviewRepository : IReviewRepository
{
    private readonly CampaignsDbContext _dbContext;

    public ReviewRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Review?> GetByIdAsync(ReviewId reviewId, CancellationToken cancellationToken)
    {
        return await this._dbContext.Reviews
            .Where(review => review.Id == reviewId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Review?> GetByCampaignIdAndClientIdAsync(CampaignId campaignId, BloggerId clientId,
        CancellationToken cancellationToken)
    {
        return await this._dbContext.Reviews
            .Where(review => review.CampaignId == campaignId && review.ClientId == clientId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Review review)
    {
        this._dbContext.Reviews.Add(review);
    }

    public void Remove(Review review)
    {
        this._dbContext.Reviews.Remove(review);
    }
}
