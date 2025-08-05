using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;

public sealed class CampaignCompletedDomainEvent(CampaignId campaignId, DateTimeOffset completedAtUtc) : DomainEvent
{
    public CampaignId CampaignId { get; init; } = campaignId;
    
    public DateTimeOffset CompletedAtUtc { get; init; } = completedAtUtc;
}
