using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.BloggerActivity.TrackReviewCreated;
using Lanka.Modules.Campaigns.IntegrationEvents.Reviews;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Campaigns;

internal sealed class ReviewCreatedIntegrationEventHandler
    : IntegrationEventHandler<ReviewCreatedIntegrationEvent>
{
    private readonly ISender _sender;

    public ReviewCreatedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        ReviewCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new TrackReviewCreatedCommand(
                integrationEvent.ClientId,
                integrationEvent.CampaignId,
                integrationEvent.Rating,
                integrationEvent.CreatedOnUtc
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(TrackReviewCreatedCommand), result.Error);
        }
    }
}
