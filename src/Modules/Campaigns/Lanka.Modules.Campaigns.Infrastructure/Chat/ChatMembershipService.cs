using Lanka.Common.Application.Notifications;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Chat;

internal sealed class ChatMembershipService : IChatMembershipService
{
    private readonly CampaignsDbContext _dbContext;

    public ChatMembershipService(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<bool> CanJoinChatAsync(
        Guid threadId,
        Guid bloggerId,
        CancellationToken cancellationToken = default)
    {
        var id = new ChatThreadId(threadId);
        var participantId = new BloggerId(bloggerId);

        return await this._dbContext.ChatThreads
            .AnyAsync(
                thread =>
                    thread.Id == id &&
                    (thread.ParticipantAId == participantId || thread.ParticipantBId == participantId),
                cancellationToken);
    }
}
