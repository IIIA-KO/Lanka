using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Complete
{
    public sealed record CompleteCampaignCommand(CampaignId CampaignId) : ICommand;
}
