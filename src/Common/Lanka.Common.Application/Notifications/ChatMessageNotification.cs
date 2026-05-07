namespace Lanka.Common.Application.Notifications;

public sealed record ChatMessageNotification(
    Guid Id,
    Guid ThreadId,
    Guid? SenderBloggerId,
    string SenderFirstName,
    string SenderLastName,
    string Content,
    bool IsSystem,
    bool IsDeleted,
    DateTime? EditedAtUtc,
    DateTime? ReadAtUtc,
    DateTime CreatedAtUtc);
