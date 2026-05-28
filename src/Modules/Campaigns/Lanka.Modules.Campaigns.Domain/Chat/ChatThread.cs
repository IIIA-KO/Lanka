using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public class ChatThread : Entity<ChatThreadId>
{
    public BloggerId ParticipantAId { get; private set; }

    public BloggerId ParticipantBId { get; private set; }

    public CampaignId? CampaignId { get; private set; }

    public OfferId? OfferId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private ChatThread()
    {
    }

    public static Result<ChatThread> Create(
        BloggerId firstParticipantId,
        BloggerId secondParticipantId,
        DateTimeOffset utcNow,
        CampaignId? campaignId = null,
        OfferId? offerId = null
    )
    {
        if (firstParticipantId == secondParticipantId)
        {
            return Result.Failure<ChatThread>(ChatThreadErrors.SameParticipant);
        }

        return new ChatThread
        {
            Id = ChatThreadId.New(),
            ParticipantAId = firstParticipantId,
            ParticipantBId = secondParticipantId,
            CampaignId = campaignId,
            OfferId = offerId,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public bool HasParticipant(BloggerId bloggerId)
    {
        return this.ParticipantAId == bloggerId || this.ParticipantBId == bloggerId;
    }

    public BloggerId GetOtherParticipant(BloggerId bloggerId)
    {
        return this.ParticipantAId == bloggerId ? this.ParticipantBId : this.ParticipantAId;
    }

    public void Touch(DateTimeOffset utcNow)
    {
        this.UpdatedAtUtc = utcNow;
    }
}
