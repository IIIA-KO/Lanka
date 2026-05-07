namespace Lanka.Common.Application.Notifications;

public interface IChatMembershipService
{
    Task<bool> CanJoinChatAsync(Guid threadId, Guid bloggerId, CancellationToken cancellationToken = default);
}
