using Lanka.Common.Domain;
using Lanka.Modules.Users.Domain.Offers;
using Lanka.Modules.Users.Domain.Offers.Names;
using Lanka.Modules.Users.Domain.Pacts.Contents;
using Lanka.Modules.Users.Domain.Pacts.DomainEvents;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Domain.Pacts
{
    public class Pact : Entity<PactId>
    {
        private readonly List<Offer> _offers = [];
        
        public PactStatus Status { get; private set; }
        
        public DateTimeOffset LastUpdatedOnUtc { get; private set; }
        
        public UserId UserId { get; init;  }
        
        public User? User { get; init;  }
        
        public Content Content { get; private set; }
        
        public IReadOnlyCollection<Offer> Offers => this._offers.AsReadOnly();
        
        private Pact() { }

        private Pact(
            PactId id,
            UserId userId,
            Content content
        )
        {
            this.Id = id;
            this.UserId = userId;
            this.Content = content;
            this.Status = PactStatus.Draft;
            this.LastUpdatedOnUtc = DateTimeOffset.UtcNow;
        }
        
        public static Result<Pact> Create(
            UserId userId,
            string content
        )
        {
            Result<Content> validationResult = Validate(content);

            if (validationResult.IsFailure)
            {
                return Result.Failure<Pact>(validationResult.Error);
            }
            
            var pact = new Pact(PactId.New(), userId, validationResult.Value);

            return pact;
        }

        public Result Publish()
        {
            if (this.Status == PactStatus.Published)
            {
                return Result.Failure(PactErrors.AlreadyPublished);
            }
            
            this.Status = PactStatus.Published;
            this.LastUpdatedOnUtc = DateTimeOffset.UtcNow;

            return Result.Success();
        }

        public Result Update(string content)
        {
            Result<Content> validationResult = Validate(content);

            if (validationResult.IsFailure)
            {
                return Result.Failure<Pact>(validationResult.Error);
            }

            this.Content = validationResult.Value;
            
            this.RaiseDomainEvent(new PactUpdatedDomainEvent(this.Id));
            
            return Result.Success();
        }

        public bool HasOffer(Name offerName)
        {
            return this._offers.Exists(offer => 
                string.Equals(offer.Name.Value, offerName.Value, StringComparison.Ordinal)
            );
        }
        
        private static Result<Content> Validate(string content)
        {
            Result<Content> contentResult = Content.Create(content);

            if (contentResult.IsFailure)
            {
                return Result.Failure<Content>(
                    ValidationError.FromResults([contentResult.Error])
                );
            }
            
            return contentResult.Value;
        }
    }
}
