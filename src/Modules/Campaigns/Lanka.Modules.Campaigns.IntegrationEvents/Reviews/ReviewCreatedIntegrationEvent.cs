using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Campaigns.IntegrationEvents.Reviews;

public class ReviewCreatedIntegrationEvent : IntegrationEvent
{
    public ReviewCreatedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid campaignId,
        Guid offerId,
        Guid clientId,
        Guid creatorId,
        Guid reviewId,
        int rating,
        string comment,
        DateTimeOffset createdOnUtc
    ) 
        : base(id, occurredOnUtc)
    {
        this.CampaignId = campaignId;
        this.OfferId = offerId;
        this.ClientId = clientId;
        this.CreatorId = creatorId;
        this.ReviewId = reviewId;
        this.Rating = rating;
        this.Comment = comment;
        this.CreatedOnUtc = createdOnUtc;
    }

    public Guid CampaignId { get; init; }
    public Guid OfferId { get; init; }
    public Guid ClientId { get; init; }
    public Guid CreatorId { get; init; }
    public Guid ReviewId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; }
    public DateTimeOffset CreatedOnUtc { get; init; }
}
