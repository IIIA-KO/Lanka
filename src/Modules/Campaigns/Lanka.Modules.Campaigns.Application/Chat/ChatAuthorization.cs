using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat;

internal static class ChatAuthorization
{
    internal static bool IsParticipant(ChatThread thread, Guid bloggerId)
    {
        return thread.HasParticipant(new BloggerId(bloggerId));
    }
}
