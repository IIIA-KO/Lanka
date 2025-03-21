using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Reject;

public sealed record RejectCampaignCommand(CampaignId CampaignId) : ICommand;
