namespace Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;

public class CampaignCompletedIntegrationEvent : CampaignIntegrationEvent
{
    public CampaignCompletedIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc, 
        Guid campaignId, 
        Guid offerId, 
        Guid clientId,
        Guid creatorId
    ) : base(id, occurredOnUtc, campaignId, offerId, clientId, creatorId) { }
}
