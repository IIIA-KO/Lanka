using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents
{
    public sealed class CampaignRejectedDomainEvent(CampaignId CampaignId) : DomainEvent;
}
