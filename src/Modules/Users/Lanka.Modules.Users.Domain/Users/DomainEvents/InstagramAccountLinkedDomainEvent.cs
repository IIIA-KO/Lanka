using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.DomainEvents;

public sealed class InstagramAccountLinkedDomainEvent(UserId userId, string email, string code) : DomainEvent
{
    public UserId UserId { get; init; } = userId;

    public string Email { get; init; } = email;

    public string Code { get; init; } = code;
}
