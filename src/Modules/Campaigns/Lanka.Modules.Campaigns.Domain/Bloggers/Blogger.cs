using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Domain.Bloggers
{
    public class Blogger : Entity<BloggerId>
    {
        public FirstName FirstName { get; private set; }

        public LastName LastName { get; private set; }

        public Email Email { get; private set; }
        
        public BirthDate BirthDate { get; private set; }
        
        public Pact? Pact { get; init; }
        
        private Blogger() { }
        
        public Blogger(
            Guid bloggerId,
            string firstName,
            string lastName,
            string email,
            DateOnly birthDate
        )
        {
            this.Id = new BloggerId(bloggerId);
            this.FirstName = new FirstName(firstName);
            this.LastName = new LastName(lastName);
            this.Email = new Email(email);
            this.BirthDate = new BirthDate(birthDate);
        }
    }
}
