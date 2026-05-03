using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;

public sealed class CampaignNotificationIntegrationEvent : IntegrationEvent
{
    public CampaignNotificationIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid recipientUserId,
        Guid campaignId,
        string campaignName,
        string newStatus
    ) : base(id, occurredOnUtc)
    {
        this.RecipientUserId = recipientUserId;
        this.CampaignId = campaignId;
        this.CampaignName = campaignName;
        this.NewStatus = newStatus;
    }

    public Guid RecipientUserId { get; init; }
    public Guid CampaignId { get; init; }
    public string CampaignName { get; init; }
    public string NewStatus { get; init; }
}
