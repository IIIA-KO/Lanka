namespace Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;

public class CampaignMarkedAsDoneIntegrationEvent : CampaignIntegrationEvent
{
    public CampaignMarkedAsDoneIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc, 
        Guid campaignId, 
        Guid offerId, 
        Guid clientId,
        Guid creatorId
    ) : base(id, occurredOnUtc, campaignId, offerId, clientId, creatorId) { }
}
