namespace Lanka.Common.Application.Notifications;

public sealed record ChatMessageDeletedNotification(
    Guid ThreadId,
    Guid MessageId);
