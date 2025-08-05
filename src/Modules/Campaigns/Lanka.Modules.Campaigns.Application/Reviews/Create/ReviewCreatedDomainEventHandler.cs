using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Reviews.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Reviews;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Reviews.Create;

internal sealed class ReviewCreatedDomainEventHandler
    : DomainEventHandler<ReviewCreatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;

    public ReviewCreatedDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        ReviewCreatedDomainEvent notification, 
        CancellationToken cancellationToken = default
    )
    {
        Result<ReviewResponse> result = await this._sender.Send(
            new GetReviewQuery(notification.ReviewId.Value),
            cancellationToken
        );
        
        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetReviewQuery), CampaignErrors.NotFound);
        }
        
        await this._eventBus.PublishAsync(
            new ReviewCreatedIntegrationEvent(
                notification.Id,
                notification.OccurredOnUtc,
                result.Value.CampaignId,
                result.Value.OfferId,
                result.Value.ClientId,
                result.Value.CreatorId,
                notification.ReviewId.Value,
                result.Value.Rating,
                result.Value.Comment,
                result.Value.CreatedOnUtc
            ),
            cancellationToken
        );
    }
}
