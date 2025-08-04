namespace Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;

public class CampaignCompletedIntegrationEvent : CampaignIntegrationEvent
{
    public CampaignCompletedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid campaignId,
        Guid offerId,
        Guid clientId,
        Guid creatorId,
        DateTimeOffset completedAtUtc
    ) : base(id, occurredOnUtc, campaignId, offerId, clientId, creatorId)
    {
        this.CompletedAtUtc = completedAtUtc;
    }
    
    public DateTimeOffset CompletedAtUtc { get; init; }
}
