using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public interface IChatThreadRepository
{
    Task<ChatThread?> GetByIdAsync(ChatThreadId id, CancellationToken cancellationToken = default);

    Task<ChatThread?> GetByParticipantsAsync(
        BloggerId firstParticipantId,
        BloggerId secondParticipantId,
        CampaignId? campaignId,
        OfferId? offerId,
        CancellationToken cancellationToken = default
    );

    void Add(ChatThread thread);
}
