using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken cancellationToken = default);

    void Add(ChatMessage message);

    Task<int> BulkMarkReadAsync(
        ChatThreadId threadId,
        BloggerId readerBloggerId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default);
}
