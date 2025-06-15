using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Users;

internal sealed class InstagramAccountLinkingStartedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingStartedIntegrationEvent>
{
    private readonly ISender _sender;

    public InstagramAccountLinkingStartedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        InstagramAccountLinkingStartedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new FetchInstagramAccountDataCommand(
                integrationEvent.UserId,
                integrationEvent.Code
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(FetchInstagramAccountDataCommand), result.Error);
        }
    }
}
