using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.SeedCampaigns;

internal sealed class SeedCampaignsCommandHandler
    : ICommandHandler<SeedCampaignsCommand, SeedCampaignsResult>
{
    private readonly ICampaignSeedingService _seedingService;

    public SeedCampaignsCommandHandler(ICampaignSeedingService seedingService)
    {
        this._seedingService = seedingService;
    }

    public async Task<Result<SeedCampaignsResult>> Handle(
        SeedCampaignsCommand request,
        CancellationToken cancellationToken
    )
    {
        int perMonth = request.CampaignsPerMonth > 0 ? request.CampaignsPerMonth : 3;

        SeedCampaignsResult result = await this._seedingService.SeedAsync(
            request.BloggerId,
            perMonth,
            cancellationToken
        );

        return result;
    }
}
