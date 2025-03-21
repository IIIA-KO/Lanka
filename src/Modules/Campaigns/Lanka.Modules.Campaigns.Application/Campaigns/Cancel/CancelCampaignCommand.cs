using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Cancel;

public sealed record CancelCampaignCommand(CampaignId CampaignId) : ICommand;
