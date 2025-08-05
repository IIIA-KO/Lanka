using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.BloggerActivity.TrackCampaignCompleted;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Campaigns;

internal sealed class CampaignCompletedIntegrationEventHandler
    : IntegrationEventHandler<CampaignCompletedIntegrationEvent>
{
    private readonly ISender _sender;

    public CampaignCompletedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        CampaignCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new TrackCampaignCompletedCommand(
                integrationEvent.ClientId,
                integrationEvent.CreatorId,
                integrationEvent.CompletedAtUtc
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(TrackCampaignCompletedCommand), result.Error);
        }
    }
}
