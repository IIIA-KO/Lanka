using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Confirm;

public sealed record ConfirmCampaignCommand(Guid CampaignId) : ICommand;
