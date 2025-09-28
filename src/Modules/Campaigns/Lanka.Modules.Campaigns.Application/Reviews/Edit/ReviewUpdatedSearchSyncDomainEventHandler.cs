using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;
using Lanka.Modules.Campaigns.Domain.Reviews.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Reviews;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Reviews.Edit;

internal sealed class ReviewUpdatedSearchSyncDomainEventHandler
    : DomainEventHandler<ReviewUpdatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private static readonly string[] _tags = ["review", "feedback"];

    public ReviewUpdatedSearchSyncDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        ReviewUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<ReviewResponse> result = await this._sender.Send(
            new GetReviewQuery(domainEvent.Id),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetReviewQuery), result.Error);
        }

        var integrationEvent = new ReviewSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.ReviewId.Value,
            SearchSyncOperation.Update,
            title: $"Review {result.Value.Id}",
            content: result.Value.Comment,
            _tags,
            metadata: new Dictionary<string, object>
            {
                { "Rating", result.Value.Rating },
                { "ClientId", result.Value.ClientId.ToString() },
                { "CreatorId", result.Value.CreatorId.ToString() },
                { "CampaignId", result.Value.CampaignId.ToString() },
                { "OfferId", result.Value.OfferId.ToString() },
                { "CreatedOnUtc", result.Value.CreatedOnUtc.ToString("O") }
            }
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
