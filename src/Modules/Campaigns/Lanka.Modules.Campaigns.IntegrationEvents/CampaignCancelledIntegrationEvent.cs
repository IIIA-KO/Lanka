namespace Lanka.Modules.Campaigns.IntegrationEvents;

public class CampaignCancelledIntegrationEvent : CampaignIntegrationEvent
{
    public CampaignCancelledIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc, 
        Guid campaignId, 
        Guid offerId, 
        Guid clientId,
        Guid creatorId
    ) : base(id, occurredOnUtc, campaignId, offerId, clientId, creatorId) { }
}
