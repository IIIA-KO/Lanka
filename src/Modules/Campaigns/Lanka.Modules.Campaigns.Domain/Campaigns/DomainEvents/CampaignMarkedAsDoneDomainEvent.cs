using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;

public sealed class CampaignMarkedAsDoneDomainEvent(CampaignId campaignId) : DomainEvent
{
    public CampaignId CampaignId { get; init; } = campaignId;
}
