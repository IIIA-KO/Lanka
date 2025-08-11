using Lanka.Common.Application.Caching;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Notifications;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramAccountLinkingCompletedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingCompletedIntegrationEvent>
{
    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public InstagramAccountLinkingCompletedIntegrationEventHandler(
        ICacheService cacheService,
        INotificationService notificationService)
    {
        this._cacheService = cacheService;
        this._notificationService = notificationService;
    }

    public override async Task Handle(
        InstagramAccountLinkingCompletedIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default
    )
    {
        string userId = integrationEvent.UserId.ToString();
        string cacheKey = $"instagram_linking_status_{userId}";
        
        Console.WriteLine($"🎉 Handling Instagram linking completion for user: {userId}");

        // Update status in cache
        var completedStatus = new InstagramOperationStatus(
            InstagramOperationType.Linking,
            InstagramOperationStatuses.Completed,
            "Instagram account linked successfully",
            DateTime.UtcNow, // This should ideally be from the original status
            DateTime.UtcNow
        );

        await this._cacheService.SetAsync(cacheKey, completedStatus, TimeSpan.FromMinutes(10), cancellationToken);

        // Send SignalR notification
        await this._notificationService.SendInstagramLinkingStatusAsync(
            userId,
            InstagramOperationStatuses.Completed,
            "Instagram account linked successfully",
            cancellationToken
        );
    }
}
