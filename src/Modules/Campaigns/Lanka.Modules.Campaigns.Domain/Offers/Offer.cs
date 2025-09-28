using Lanka.Common.Contracts.Currencies;
using Lanka.Common.Contracts.Monies;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Offers.Descriptions;
using Lanka.Modules.Campaigns.Domain.Offers.DomainEvents;
using Lanka.Modules.Campaigns.Domain.Offers.Names;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Domain.Offers;

public class Offer : Entity<OfferId>
{
    public PactId PactId { get; init; }

    public Pact? Pact { get; init; }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public Money Price { get; private set; }

    public DateTimeOffset? LastCooperatedOnUtc { get; private set; }

    private Offer() { }

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
        decimal priceAmount,
        string priceCurrency)
    {
        Result<(Name, Description, Money)> validationResult =
            Validate(name, description, priceAmount, priceCurrency);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Offer>(validationResult.Error);
        }

        (Name _name, Description _description, Money price) = validationResult.Value;

        var offer = new Offer(OfferId.New(), pactId, _name, _description, price);
        
        offer.RaiseDomainEvent(new OfferCreatedDomainEvent(offer.Id));

        return offer;
    }

    public Result Update(
        string name,
        string description,
        decimal priceAmount,
        string priceCurrency
    )
    {
        Result<(Name, Description, Money)> validationResult =
            Validate(name, description, priceAmount, priceCurrency);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        (this.Name, this.Description, this.Price) = validationResult.Value;
        
        this.RaiseDomainEvent(new OfferUpdatedDomainEvent(this.Id));

        return Result.Success();
    }

    private static Result<(Name, Description, Money)> Validate(
        string name,
        string description,
        decimal priceAmount,
        string priceCurrency
    )
    {
        Result<Name> nameResult = Name.Create(name);
        Result<Description> descriptionResult = Description.Create(description);
        Result<Money> priceResult = Money.Create(priceAmount, Currency.FromCode(priceCurrency));

        return new ValidationBuilder()
            .Add(nameResult)
            .Add(descriptionResult)
            .Add(priceResult)
            .Build(() =>
                (
                    nameResult.Value,
                    descriptionResult.Value,
                    priceResult.Value
                )
            );
    }

    public void SetLastCooperatedOnUtc(DateTimeOffset utcNow)
    {
        this.LastCooperatedOnUtc = utcNow;
    }
    
    public void Delete()
    {
        this.RaiseDomainEvent(new OfferDeletedDomainEvent(this.Id));
    }
}
