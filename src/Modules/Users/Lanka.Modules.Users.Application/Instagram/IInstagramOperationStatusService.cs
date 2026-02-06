using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Application.Instagram;

/// <summary>
/// Centralized service for managing Instagram operation status.
/// Handles cache key generation and notification dispatch to avoid duplication and errors.
/// </summary>
public interface IInstagramOperationStatusService
{
    /// <summary>
    /// Updates the status of an Instagram operation in cache and sends SignalR notification.
    /// </summary>
    Task SetStatusAsync(
        Guid userId,
        string operationType,
        string status,
        string? message,
        DateTime? startedAt = null,
        DateTime? completedAt = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the current status of an Instagram operation from cache.
    /// Returns a "not_found" status if no operation is in progress.
    /// </summary>
    Task<InstagramOperationStatus> GetStatusAsync(
        Guid userId,
        string operationType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates the cache key for an Instagram operation.
    /// </summary>
    static string GetCacheKey(Guid userId, string operationType) =>
        $"instagram_{operationType}_status_{userId}";
}
