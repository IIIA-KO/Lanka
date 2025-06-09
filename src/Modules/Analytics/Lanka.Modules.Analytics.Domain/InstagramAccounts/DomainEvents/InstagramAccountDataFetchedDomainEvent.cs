using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;

public sealed class InstagramAccountDataFetchedDomainEvent(UserId userId, string username, string providerId) : DomainEvent
{
    public UserId UserId { get; init; } = userId;
    public string Username { get; init; } = username;
    public string ProviderId { get; init; } = providerId;
}
