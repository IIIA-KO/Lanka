using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Complete;

public sealed record CompleteCampaignCommand(Guid CampaignId) : ICommand;
