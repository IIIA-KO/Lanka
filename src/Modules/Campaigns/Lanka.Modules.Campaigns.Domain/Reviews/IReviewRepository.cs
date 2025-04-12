using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Domain.Reviews;

public interface IReviewRepository
{
    Task<Review?> GetByCampaignIdAndClientIdAsync(
        CampaignId campaignId,
        BloggerId clientId,
        CancellationToken cancellationToken
    );

    Task<Review?> GetByIdAsync(ReviewId reviewId, CancellationToken cancellationToken);

    void Add(Review review);
    void Remove(Review review);
}
