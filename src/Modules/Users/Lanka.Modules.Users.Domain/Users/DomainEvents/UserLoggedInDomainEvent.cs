using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserLoggedInDomainEvent(UserId userId, DateTimeOffset lastLoggedInAtUtc) : DomainEvent
{
    public UserId UserId { get; init; } = userId;
    public DateTimeOffset LastLoggedInAtUtc { get; init; } = lastLoggedInAtUtc;
}
