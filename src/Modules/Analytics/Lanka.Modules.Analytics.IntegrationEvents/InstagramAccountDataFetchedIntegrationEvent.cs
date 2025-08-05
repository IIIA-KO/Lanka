using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Analytics.IntegrationEvents;

public sealed class InstagramAccountDataFetchedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountDataFetchedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string username,
        int followersCount,
        int mediaCount,
        string providerId
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Username = username;
        this.FollowersCount = followersCount;
        this.MediaCount = mediaCount;
        this.ProviderId = providerId;
    }

    public Guid UserId { get; }

    public string Username { get; }

    public int FollowersCount { get; init; }
    
    public int MediaCount { get; init; }
    
    public string ProviderId { get; }
}
