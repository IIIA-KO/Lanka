using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat;

internal static class CampaignChatThreadResolver
{
    internal static async Task<Result<ChatThread>> GetOrCreateAsync(
        IChatThreadRepository chatThreadRepository,
        CampaignResponse campaign,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var clientId = new BloggerId(campaign.ClientId);
        var creatorId = new BloggerId(campaign.CreatorId);
        var campaignId = new CampaignId(campaign.Id);

        ChatThread? thread = await chatThreadRepository.GetByParticipantsAsync(
            clientId,
            creatorId,
            campaignId,
            null,
            cancellationToken);

        if (thread is not null)
        {
            thread.Touch(utcNow);
            return thread;
        }

        Result<ChatThread> threadResult = ChatThread.Create(
            clientId,
            creatorId,
            utcNow,
            campaignId);

        if (threadResult.IsFailure)
        {
            return Result.Failure<ChatThread>(threadResult.Error);
        }

        chatThreadRepository.Add(threadResult.Value);
        return threadResult.Value;
    }
}
