using Lanka.Common.Contracts.Monies;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Offers.Descriptions;
using Lanka.Modules.Campaigns.Domain.Offers.Names;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Domain.Offers
{
    public class Offer : Entity<OfferId>
    {
        public PactId PactId { get; init; }
        
        public Pact? Pact { get; init; }
        
        public Name Name { get; private set; }

        public Description Description { get; private set; }
        
        public Money Price { get; private set; }
        
        public DateTimeOffset? LastCooperatedOnUtc { get; private set; }
        
        private Offer() {}

        private Offer(
            OfferId id,
            PactId pactId,
            Name name,
            Description description,
            Money price
        )
        {
            this.Id = id;
            this.PactId = pactId;
            this.Name = name;
            this.Description = description;
            this.Price = price;
        }

        public static Result<Offer> Create(
            PactId pactId,
            string name,
            string description,
            Money price)
        {
            Result<(Name, Description)> validationResult = Validate(name, description);

            if (validationResult.IsFailure)
            {
                return Result.Failure<Offer>(validationResult.Error);
            }
            
            (Name n, Description d) = validationResult.Value;
            
            var offer = new Offer(OfferId.New(), pactId, n, d, price);
            
            return offer;
        }
        
        private static Result<(Name, Description)> Validate(
            string name,
            string description
        )
        {
            Result<Name> nameResult = Name.Create(name);
            Result<Description> descriptionResult = Description.Create(description);
            
            if (nameResult.IsFailure 
                || descriptionResult.IsFailure)
            {
                return Result.Failure<(Name, Description)>(
                    ValidationError.FromResults([nameResult, descriptionResult])
                );
            }

            return (
                nameResult.Value,
                descriptionResult.Value
            );
        }

        public void UpdateOffer(DateTimeOffset utcNow)
        {
            this.LastCooperatedOnUtc = utcNow;
        }
    }
}
