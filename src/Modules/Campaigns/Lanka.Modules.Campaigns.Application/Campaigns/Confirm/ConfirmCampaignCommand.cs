using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Confirm;

public sealed record ConfirmCampaignCommand(CampaignId CampaignId) : ICommand;
