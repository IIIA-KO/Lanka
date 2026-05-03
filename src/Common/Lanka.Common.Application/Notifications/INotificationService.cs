namespace Lanka.Common.Application.Notifications;

public interface INotificationService
{
    Task SendInstagramLinkingStatusAsync(string userId, string status, string? message = null, CancellationToken cancellationToken = default);
    Task SendInstagramRenewalStatusAsync(string userId, string status, string? message = null, CancellationToken cancellationToken = default);
    Task SendCampaignNotificationAsync(string identityId, Guid campaignId, string campaignName, string newStatus, CancellationToken cancellationToken = default);
}
