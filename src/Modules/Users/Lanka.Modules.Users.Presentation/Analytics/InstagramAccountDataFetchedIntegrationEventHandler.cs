using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Users.Application.Instagram.FinishLinking;
using MediatR;

namespace Lanka.Modules.Users.Presentation.Analytics;

internal sealed class InstagramAccountDataFetchedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountDataFetchedIntegrationEvent>
{
    private readonly ISender _sender;

    public InstagramAccountDataFetchedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        InstagramAccountDataFetchedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new FinishInstagramLinkingCommand(
                integrationEvent.UserId,
                integrationEvent.Username,
                integrationEvent.ProviderId
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(FinishInstagramLinkingCommand), result.Error);
        }
    }
}
