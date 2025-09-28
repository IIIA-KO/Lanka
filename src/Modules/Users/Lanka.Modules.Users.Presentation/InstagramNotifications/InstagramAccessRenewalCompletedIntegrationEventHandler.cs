using Lanka.Common.Application.Caching;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Notifications;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramAccessRenewalCompletedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccessRenewalCompletedIntegrationEvent>
{
    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public InstagramAccessRenewalCompletedIntegrationEventHandler(
        ICacheService cacheService,
        INotificationService notificationService)
    {
        this._cacheService = cacheService;
        this._notificationService = notificationService;
    }

    public override async Task Handle(
        InstagramAccessRenewalCompletedIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default
    )
    {
        string userId = integrationEvent.UserId.ToString();
        string cacheKey = $"instagram_renewal_status_{userId}";

        // Update status in cache
        var completedStatus = new InstagramOperationStatus(
            InstagramOperationType.Renewal,
            InstagramOperationStatuses.Completed,
            "Instagram access renewed successfully",
            DateTime.UtcNow, // This should ideally be from the original status
            DateTime.UtcNow
        );

        await this._cacheService.SetAsync(cacheKey, completedStatus, TimeSpan.FromMinutes(10), cancellationToken);

        // Send SignalR notification
        await this._notificationService.SendInstagramRenewalStatusAsync(
            userId,
            InstagramOperationStatuses.Completed,
            "Instagram access renewed successfully",
            cancellationToken
        );
    }
}
