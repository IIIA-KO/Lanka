namespace Lanka.Modules.Campaigns.IntegrationEvents;

public class CampaignPendedIntegrationEvent : CampaignIntegrationEvent
{
    public CampaignPendedIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc, 
        Guid campaignId, 
        Guid offerId, 
        Guid clientId,
        Guid creatorId
    ) : base(id, occurredOnUtc, campaignId, offerId, clientId, creatorId) { }
}
