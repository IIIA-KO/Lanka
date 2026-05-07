namespace Lanka.Common.Application.Notifications;

public sealed record ChatMessagesReadNotification(
    Guid ThreadId,
    Guid ReaderBloggerId,
    DateTime ReadAtUtc);
