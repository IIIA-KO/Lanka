using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.MarkAsDone
{
    public sealed record MarkCampaignAsDoneCommand(CampaignId CampaignId) : ICommand;
}
