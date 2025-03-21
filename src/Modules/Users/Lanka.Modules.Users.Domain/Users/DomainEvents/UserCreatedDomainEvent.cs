using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserCreatedDomainEvent(UserId userId) : DomainEvent
{
    public UserId UserId { get; init; } = userId;
}