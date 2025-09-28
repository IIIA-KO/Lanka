using Lanka.Common.Application.Caching;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Notifications;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramRenewalAccessFailureCleanedUpIntegrationEventHandler
    : IntegrationEventHandler<InstagramRenewalAccessFailureCleanedUpIntegrationEvent>
{
    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public InstagramRenewalAccessFailureCleanedUpIntegrationEventHandler(
        ICacheService cacheService,
        INotificationService notificationService)
    {
        this._cacheService = cacheService;
        this._notificationService = notificationService;
    }

    public override async Task Handle(
        InstagramRenewalAccessFailureCleanedUpIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        // CorrelationId is the UserId in this context
        string userId = integrationEvent.CorrelationId.ToString();
        string cacheKey = $"instagram_renewal_status_{userId}";

        // Update status in cache
        var failedStatus = new InstagramOperationStatus(
            InstagramOperationType.Renewal,
            InstagramOperationStatuses.Failed,
            integrationEvent.Reason,
            DateTime.UtcNow, // This should ideally be from the original status
            DateTime.UtcNow
        );

        await this._cacheService.SetAsync(cacheKey, failedStatus, TimeSpan.FromMinutes(10), cancellationToken);

        // Send SignalR notification
        await this._notificationService.SendInstagramRenewalStatusAsync(
            userId,
            InstagramOperationStatuses.Failed,
            integrationEvent.Reason,
            cancellationToken
        );
    }
}
