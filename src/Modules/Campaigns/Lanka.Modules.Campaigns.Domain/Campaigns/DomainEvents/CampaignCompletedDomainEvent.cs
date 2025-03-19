using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents
{
    public sealed class CampaignCompletedDomainEvent(CampaignId CampaignId) : DomainEvent;
}
