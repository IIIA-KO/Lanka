namespace Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;

public class CampaignConfirmedIntegrationEvent : CampaignIntegrationEvent
{
    public CampaignConfirmedIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc, 
        Guid campaignId, 
        Guid offerId, 
        Guid clientId,
        Guid creatorId
    ) : base(id, occurredOnUtc, campaignId, offerId, clientId, creatorId) { }
}
