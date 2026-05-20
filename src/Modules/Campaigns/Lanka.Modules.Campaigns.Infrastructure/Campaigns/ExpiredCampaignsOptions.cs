namespace Lanka.Modules.Campaigns.Infrastructure.Campaigns;

internal sealed class ExpiredCampaignsOptions
{
    public int IntervalInSeconds { get; init; } = 300;

    public int BatchSize { get; init; } = 500;
}
