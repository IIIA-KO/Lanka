using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.SeedCampaigns;

public sealed record SeedCampaignsCommand(Guid BloggerId, int CampaignsPerMonth)
    : ICommand<SeedCampaignsResult>;
