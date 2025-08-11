namespace Lanka.Common.Application.Notifications;

public interface INotificationService
{
    Task SendInstagramLinkingStatusAsync(string userId, string status, string? message = null, CancellationToken cancellationToken = default);
    Task SendInstagramRenewalStatusAsync(string userId, string status, string? message = null, CancellationToken cancellationToken = default);
}
