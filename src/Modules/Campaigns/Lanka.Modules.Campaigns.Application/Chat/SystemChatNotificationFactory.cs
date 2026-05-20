using Lanka.Common.Application.Notifications;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat;

internal static class SystemChatNotificationFactory
{
    internal static ChatMessageNotification Create(ChatMessage message)
    {
        return new ChatMessageNotification(
            message.Id.Value,
            message.ThreadId.Value,
            message.SenderBloggerId?.Value,
            string.Empty,
            string.Empty,
            message.Content,
            message.IsSystem,
            message.IsDeleted,
            message.EditedAtUtc?.UtcDateTime,
            message.ReadAtUtc?.UtcDateTime,
            message.CreatedAtUtc.UtcDateTime);
    }
}
