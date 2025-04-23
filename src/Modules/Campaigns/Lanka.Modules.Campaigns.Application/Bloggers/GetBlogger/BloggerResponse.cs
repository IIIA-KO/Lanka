using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;

public sealed class BloggerResponse
{
    public Guid Id { get; init; }
    
    public string FirstName { get; init; } 
    
    public string LastName { get; init; }
    
    public string Email { get; init; }
    
    public DateOnly BirthDate { get; init; }
    
    public Guid? PactId { get; init; }
    
    public static BloggerResponse FromBloger(Blogger blogger)
    {
        return new BloggerResponse
        {
            Id = blogger.Id.Value,
            FirstName = blogger.FirstName.Value,
            LastName = blogger.LastName.Value,
            Email = blogger.Email.Value,
            BirthDate = blogger.BirthDate.Value,
            PactId = blogger.Pact?.Id.Value
        };
    }
}
