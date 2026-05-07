using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Chat;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Chat;

internal sealed class ChatThreadRepository : IChatThreadRepository
{
    private readonly CampaignsDbContext _dbContext;

    public ChatThreadRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<ChatThread?> GetByIdAsync(ChatThreadId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.ChatThreads
            .Where(thread => thread.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ChatThread?> GetByParticipantsAsync(
        BloggerId firstParticipantId,
        BloggerId secondParticipantId,
        CampaignId? campaignId,
        OfferId? offerId,
        CancellationToken cancellationToken = default)
    {
        return await this._dbContext.ChatThreads
            .Where(thread =>
                (thread.ParticipantAId == firstParticipantId && thread.ParticipantBId == secondParticipantId ||
                 thread.ParticipantAId == secondParticipantId && thread.ParticipantBId == firstParticipantId) &&
                thread.CampaignId == campaignId &&
                thread.OfferId == offerId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(ChatThread thread)
    {
        this._dbContext.ChatThreads.Add(thread);
    }
}
