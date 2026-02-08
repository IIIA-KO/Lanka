using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Instagram.RefreshToken;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Users;

internal sealed class InstagramAccessRenewalStartedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccessRenewalStartedIntegrationEvent>
{
    private const string StatusProcessing = "processing";

    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private readonly INotificationService _notificationService;
    private readonly IInstagramUserContext? _instagramUserContext;
    private readonly IInstagramAccountRepository _instagramAccountRepository;

    public InstagramAccessRenewalStartedIntegrationEventHandler(
        ISender sender,
        IEventBus eventBus,
        INotificationService notificationService,
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramUserContext? instagramUserContext = null)
    {
        this._sender = sender;
        this._eventBus = eventBus;
        this._notificationService = notificationService;
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramUserContext = instagramUserContext;
    }

    public override async Task Handle(
        InstagramAccessRenewalStartedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        string userId = integrationEvent.UserId.ToString();

        // Look up the existing Instagram account to get the user's email for service resolution
        InstagramAccount? existingAccount = await this._instagramAccountRepository.GetByUserIdAsync(
            new UserId(integrationEvent.UserId),
            cancellationToken
        );

        if (existingAccount is not null)
        {
            // Set email in ambient context before dispatching command (for Development service resolution)
            this._instagramUserContext?.SetEmail(existingAccount.Email.Value);
        }

        // Send "processing" notification via SignalR before starting the command
        await this._notificationService.SendInstagramRenewalStatusAsync(
            userId,
            StatusProcessing,
            "Refreshing Instagram access token...",
            cancellationToken
        );

        Result result = await this._sender.Send(
            new RefreshInstagramTokenCommand(
                integrationEvent.UserId,
                integrationEvent.Code
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            // Publish failure event instead of throwing
            await this._eventBus.PublishAsync(
                new InstagramRenewalFailedIntegrationEvent(
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
