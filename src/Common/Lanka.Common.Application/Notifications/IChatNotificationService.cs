namespace Lanka.Common.Application.Notifications;

public interface IChatNotificationService
{
    Task SendMessageAsync(ChatMessageNotification message, CancellationToken cancellationToken = default);

    Task EditMessageAsync(ChatMessageNotification message, CancellationToken cancellationToken = default);

    Task DeleteMessageAsync(ChatMessageDeletedNotification notification, CancellationToken cancellationToken = default);

    Task MarkReadAsync(ChatMessagesReadNotification notification, CancellationToken cancellationToken = default);
}
