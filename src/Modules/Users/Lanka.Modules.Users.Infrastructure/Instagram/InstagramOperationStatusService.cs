using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Notifications;
using Lanka.Modules.Users.Application.Instagram;
using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Infrastructure.Instagram;

internal sealed class InstagramOperationStatusService : IInstagramOperationStatusService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public InstagramOperationStatusService(
        ICacheService cacheService,
        INotificationService notificationService)
    {
        this._cacheService = cacheService;
        this._notificationService = notificationService;
    }

    public async Task SetStatusAsync(
        Guid userId,
        string operationType,
        string status,
        string? message,
        DateTime? startedAt = null,
        DateTime? completedAt = null,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = IInstagramOperationStatusService.GetCacheKey(userId, operationType);

        var operationStatus = new InstagramOperationStatus(
            operationType,
            status,
            message,
            startedAt ?? DateTime.UtcNow,
            completedAt
        );

        await this._cacheService.SetAsync(cacheKey, operationStatus, CacheDuration, cancellationToken);

        // Send SignalR notification based on operation type
        string userIdStr = userId.ToString();

        if (operationType == InstagramOperationType.Linking)
        {
            await this._notificationService.SendInstagramLinkingStatusAsync(
                userIdStr, status, message, cancellationToken);
        }
        else if (operationType == InstagramOperationType.Renewal)
        {
            await this._notificationService.SendInstagramRenewalStatusAsync(
                userIdStr, status, message, cancellationToken);
        }
    }

    public async Task<InstagramOperationStatus> GetStatusAsync(
        Guid userId,
        string operationType,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = IInstagramOperationStatusService.GetCacheKey(userId, operationType);

        InstagramOperationStatus? status = await this._cacheService.GetAsync<InstagramOperationStatus>(
            cacheKey, cancellationToken);

        if (status is null)
        {
            return new InstagramOperationStatus(
                operationType,
                InstagramOperationStatuses.NotFound,
                $"No {operationType} operation in progress.",
                null,
                null
            );
        }

        return status;
    }
}
