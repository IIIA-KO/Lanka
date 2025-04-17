using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Reject;

public sealed record RejectCampaignCommand(Guid CampaignId) : ICommand;
