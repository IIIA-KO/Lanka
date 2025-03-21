using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;

public sealed class CampaignCancelledDomainEvent(CampaignId campaignId) : DomainEvent
{
    public CampaignId CampaignId { get; init; } = campaignId;
}
