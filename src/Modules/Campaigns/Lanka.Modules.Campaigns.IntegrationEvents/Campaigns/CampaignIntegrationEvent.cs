using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;

public class CampaignIntegrationEvent : IntegrationEvent
{
    protected CampaignIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid campaignId,
        Guid offerId,
        Guid clientId,
        Guid creatorId
    )
        : base(id, occurredOnUtc)
    {
        this.CampaignId = campaignId;
        this.OfferId = offerId;
        this.ClientId = clientId;
        this.CreatorId = creatorId;
    }

    public Guid CampaignId { get; init; }
    public Guid OfferId { get; init; }
    public Guid ClientId { get; init; }
    public Guid CreatorId { get; init; }
}
