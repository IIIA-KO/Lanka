using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public class Blogger : Entity<BloggerId>
{
    public FirstName FirstName { get; private set; }

    public LastName LastName { get; private set; }

    public Email Email { get; private set; }

    public BirthDate BirthDate { get; private set; }

    public Pact? Pact { get; init; }

    private Blogger() { }
    
    public static Blogger Create(
        Guid bloggerId,
        string firstName,
        string lastName,
        string email,
        DateOnly birthDate
    )
    {
        return new Blogger
        {
            Id = new BloggerId(bloggerId),
            FirstName = new FirstName(firstName),
            LastName = new LastName(lastName),
            Email = new Email(email),
            BirthDate = new BirthDate(birthDate)
        };
    }
}
