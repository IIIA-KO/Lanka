using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.DomainEvents;

public sealed class InstagramAccessRenewedDomainEvent(UserId userId, string code) : DomainEvent
{
    public UserId UserId { get; init; } = userId;

    public string Code { get; init; } = code;
}
