using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.DomainEvents;

public class UserDeletedDomainEvent(UserId userId) : DomainEvent
{
    public UserId UserId { get; init; } = userId;
}
