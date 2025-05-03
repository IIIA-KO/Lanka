using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;

public sealed class BloggerUpdatedIntegrationEvent : IntegrationEvent
{
    public BloggerUpdatedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid bloggerId,
        string firstName,
        string lastName,
        DateOnly birthDate,
        string bio
    ) : base(id, occurredOnUtc)
    {
        this.BloggerId = bloggerId;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.BirthDate = birthDate;
        this.Bio = bio;
    }
    
    public Guid BloggerId { get; init; }
    
    public string FirstName { get; init; }
    
    public string LastName { get; init; }
    
    public DateOnly BirthDate { get; init; }
    
    public string Bio { get; init; }
}
