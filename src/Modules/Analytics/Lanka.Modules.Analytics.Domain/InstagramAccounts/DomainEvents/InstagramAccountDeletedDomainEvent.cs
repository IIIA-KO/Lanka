using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;

public sealed class InstagramAccountDeletedDomainEvent(
    InstagramAccountId instagramAccountId,
    UserId userId
) : DomainEvent
{
    public InstagramAccountId InstagramAccountId { get; init; } = instagramAccountId;
    public UserId UserId { get; init; } = userId;
}

