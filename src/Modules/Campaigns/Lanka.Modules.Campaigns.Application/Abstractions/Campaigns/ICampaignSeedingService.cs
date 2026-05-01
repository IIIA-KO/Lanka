using Lanka.Modules.Campaigns.Application.Campaigns.SeedCampaigns;

namespace Lanka.Modules.Campaigns.Application.Abstractions.Campaigns;

public interface ICampaignSeedingService
{
    Task<SeedCampaignsResult> SeedAsync(
        Guid bloggerId,
        int campaignsPerMonth,
        CancellationToken cancellationToken = default);
}
