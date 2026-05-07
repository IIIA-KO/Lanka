using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Chat;

internal sealed class ChatMessageRepository : IChatMessageRepository
{
    private readonly CampaignsDbContext _dbContext;

    public ChatMessageRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.ChatMessages
            .Where(message => message.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(ChatMessage message)
    {
        this._dbContext.ChatMessages.Add(message);
    }

    public async Task<int> BulkMarkReadAsync(
        ChatThreadId threadId,
        BloggerId readerBloggerId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        return await this._dbContext.ChatMessages
            .Where(message =>
                message.ThreadId == threadId &&
                message.SenderBloggerId != null &&
                message.SenderBloggerId != readerBloggerId &&
                !message.IsSystem &&
                !message.IsDeleted &&
                message.ReadAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(message => message.ReadAtUtc, utcNow),
                cancellationToken);
    }
}
