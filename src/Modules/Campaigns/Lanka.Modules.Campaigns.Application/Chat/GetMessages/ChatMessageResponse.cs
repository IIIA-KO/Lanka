namespace Lanka.Modules.Campaigns.Application.Chat.GetMessages;

public sealed record ChatMessageResponse(
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
