using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserUpdatedDomainEvent(
    UserId userId,
    string firstName,
    string lastName,
    DateOnly birthDate
) : DomainEvent
{
    public UserId UserId { get; init; } = userId;
    
    public string FirstName { get; init; } = firstName;
    
    public string LastName { get; init; } = lastName;
    
    public DateOnly BirthDate { get; init; } = birthDate;
}
