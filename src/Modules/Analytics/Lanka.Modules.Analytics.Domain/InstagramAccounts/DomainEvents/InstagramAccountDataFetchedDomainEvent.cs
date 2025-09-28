using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;

public sealed class InstagramAccountDataFetchedDomainEvent(
    InstagramAccountId instagramAccountId,
    UserId userId,
    string username,
    int followersCount,
    int mediaCount,
    string providerId
) : DomainEvent
{
    public InstagramAccountId InstagramAccountId { get; init; } = instagramAccountId;
    public UserId UserId { get; init; } = userId;
    public string Username { get; init; } = username;
    public int FollowersCount { get; init; } = followersCount;
    public int MediaCount { get; init; } = mediaCount;
    public string ProviderId { get; init; } = providerId;
}
