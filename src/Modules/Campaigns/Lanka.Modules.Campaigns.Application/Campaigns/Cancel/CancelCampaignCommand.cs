using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Cancel;

public sealed record CancelCampaignCommand(Guid CampaignId) : ICommand;
