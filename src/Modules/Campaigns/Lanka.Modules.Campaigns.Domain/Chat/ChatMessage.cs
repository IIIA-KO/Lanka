using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public class ChatMessage : Entity<ChatMessageId>
{
    public ChatThreadId ThreadId { get; private set; }

    public BloggerId? SenderBloggerId { get; private set; }

    public string Content { get; private set; }

    public bool IsSystem { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? EditedAtUtc { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    private ChatMessage()
    {
    }

    public static Result<ChatMessage> CreateUserMessage(
        ChatThreadId threadId,
        BloggerId senderBloggerId,
        string content,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<ChatMessage>(ChatMessageErrors.EmptyContent);
        }

        string trimmed = content.Trim();
        if (trimmed.Length > 4000)
        {
            return Result.Failure<ChatMessage>(ChatMessageErrors.ContentTooLong);
        }

        return new ChatMessage
        {
            Id = ChatMessageId.New(),
            ThreadId = threadId,
            SenderBloggerId = senderBloggerId,
            Content = trimmed,
            IsSystem = false,
            IsDeleted = false,
            CreatedAtUtc = utcNow
        };
    }

    public static ChatMessage CreateSystemMessage(
        ChatThreadId threadId,
        string content,
        DateTimeOffset utcNow)
    {
        return new ChatMessage
        {
            Id = ChatMessageId.New(),
            ThreadId = threadId,
            SenderBloggerId = null,
            Content = content,
            IsSystem = true,
            IsDeleted = false,
            CreatedAtUtc = utcNow
        };
    }

    public Result Edit(string newContent, DateTimeOffset utcNow)
    {
        if (this.IsSystem)
        {
            return Result.Failure(ChatMessageErrors.SystemMessageImmutable);
        }

        if (this.IsDeleted)
        {
            return Result.Failure(ChatMessageErrors.DeletedMessageImmutable);
        }

        if (string.IsNullOrWhiteSpace(newContent))
        {
            return Result.Failure(ChatMessageErrors.EmptyContent);
        }

        string trimmed = newContent.Trim();
        if (trimmed.Length > 4000)
        {
            return Result.Failure(ChatMessageErrors.ContentTooLong);
        }

        this.Content = trimmed;
        this.EditedAtUtc = utcNow;

        return Result.Success();
    }

    public Result SoftDelete()
    {
        if (this.IsSystem)
        {
            return Result.Failure(ChatMessageErrors.SystemMessageImmutable);
        }

        if (this.IsDeleted)
        {
            return Result.Success();
        }

        this.IsDeleted = true;
        this.Content = string.Empty;

        return Result.Success();
    }

    public void MarkRead(DateTimeOffset utcNow)
    {
        this.ReadAtUtc ??= utcNow;
    }
}
