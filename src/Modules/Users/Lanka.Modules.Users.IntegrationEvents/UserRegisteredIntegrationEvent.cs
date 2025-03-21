using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents;

public sealed class UserRegisteredIntegrationEvent : IntegrationEvent
{
    public UserRegisteredIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc,
        Guid userId,
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate
    ) 
        : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Email = email;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.BirthDate = birthDate;
    }
        
    public Guid UserId { get; init; }
        
    public string Email { get; init; }
        
    public string FirstName { get; init; }
        
    public string LastName { get; init; }
        
    public DateOnly BirthDate { get; init; }
}
