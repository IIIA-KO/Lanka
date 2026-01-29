using Lanka.Common.Application.EventBus;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Users;

internal sealed class InstagramAccountLinkingStartedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingStartedIntegrationEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private readonly IInstagramUserContext? _instagramUserContext;

    public InstagramAccountLinkingStartedIntegrationEventHandler(
        ISender sender,
        IEventBus eventBus,
        IInstagramUserContext? instagramUserContext = null
    )
    {
        this._sender = sender;
        this._eventBus = eventBus;
        this._instagramUserContext = instagramUserContext;
    }

    public override async Task Handle(
        InstagramAccountLinkingStartedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        // Set email in ambient context before dispatching command (for Development service resolution)
        this._instagramUserContext?.SetEmail(integrationEvent.Email);

        Result result = await this._sender.Send(
            new FetchInstagramAccountDataCommand(
                integrationEvent.UserId,
                integrationEvent.Email,
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
        }
    }
}
