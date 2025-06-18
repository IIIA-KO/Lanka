using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Analytics.IntegrationEvents;

public sealed class InstagramAccountDataFetchedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountDataFetchedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string username,
        string providerId
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Username = username;
        this.ProviderId = providerId;
    }

    public Guid UserId { get; }

    public string Username { get; }

    public string ProviderId { get; }
}
