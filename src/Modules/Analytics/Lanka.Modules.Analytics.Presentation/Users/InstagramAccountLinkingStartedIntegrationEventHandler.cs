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
    private readonly IEventBus _eventBus;


    public InstagramAccountLinkingStartedIntegrationEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
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
            await this._eventBus.PublishAsync(
                new InstagramLinkingFailedIntegrationEvent(
                    integrationEvent.Id,
                    integrationEvent.OccurredOnUtc,
                    integrationEvent.UserId,
                    result.Error.Description
                ),
                cancellationToken
            );

            throw new LankaException(nameof(FetchInstagramAccountDataCommand), result.Error);
        }
    }
}
