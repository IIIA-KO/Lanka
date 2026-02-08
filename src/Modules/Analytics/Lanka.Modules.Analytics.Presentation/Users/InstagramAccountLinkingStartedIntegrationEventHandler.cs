using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Users;

internal sealed class InstagramAccountLinkingStartedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingStartedIntegrationEvent>
{
    private const string StatusProcessing = "processing";

    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private readonly IInstagramUserContext? _instagramUserContext;
    private readonly INotificationService _notificationService;

    public InstagramAccountLinkingStartedIntegrationEventHandler(
        ISender sender,
        IEventBus eventBus,
        INotificationService notificationService,
        IInstagramUserContext? instagramUserContext = null
    )
    {
        this._sender = sender;
        this._eventBus = eventBus;
        this._notificationService = notificationService;
        this._instagramUserContext = instagramUserContext;
    }

    public override async Task Handle(
        InstagramAccountLinkingStartedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        // Set email in ambient context before dispatching command (for Development service resolution)
        this._instagramUserContext?.SetEmail(integrationEvent.Email);

        string userId = integrationEvent.UserId.ToString();

        // Send "processing" notification via SignalR before starting the command
        // Note: We don't write to cache here to avoid cross-module type dependency issues.
        // The cache is managed by the Users module; SignalR provides real-time updates.
        await this._notificationService.SendInstagramLinkingStatusAsync(
            userId,
            StatusProcessing,
            "Fetching Instagram account data...",
            cancellationToken
        );

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
