using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Application.Instagram;

public interface IInstagramOperationStatusService
{
    Task SetStatusAsync(
        Guid userId,
        string operationType,
        string status,
        string? message,
        DateTime? startedAt = null,
        DateTime? completedAt = null,
        CancellationToken cancellationToken = default
    );

    Task<InstagramOperationStatus> GetStatusAsync(
        Guid userId,
        string operationType,
        CancellationToken cancellationToken = default
    );

    static string GetCacheKey(Guid userId, string operationType) =>
        $"instagram_{operationType}_status_{userId}";
}
