using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.Notifications;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByRecipientAsync(BloggerId recipientId, CancellationToken cancellationToken);

    Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken cancellationToken);

    void Add(Notification notification);
}
