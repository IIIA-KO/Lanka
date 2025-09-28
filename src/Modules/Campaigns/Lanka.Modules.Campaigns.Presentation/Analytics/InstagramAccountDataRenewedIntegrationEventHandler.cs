using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Campaigns.Application.Instagram;
using MediatR;

namespace Lanka.Modules.Campaigns.Presentation.Analytics;

internal sealed class InstagramAccountDataRenewedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountDataRenewedIntegrationEvent>
{
    private readonly ISender _sender;

    public InstagramAccountDataRenewedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        InstagramAccountDataRenewedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new UpdateInstagramDataCommand(
                integrationEvent.UserId,
                integrationEvent.Username,
                integrationEvent.FollowersCount,
                integrationEvent.MediaCount
            ),
            cancellationToken
        );
        
        if (result.IsFailure)
        {
            throw new LankaException(nameof(UpdateInstagramDataCommand), result.Error);
        }
    }
}
