using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Offers.Names;
using Lanka.Modules.Campaigns.Domain.Pacts.Contents;

namespace Lanka.Modules.Campaigns.Domain.Pacts;

public sealed class Pact : Entity<PactId>
{
    private readonly List<Offer> _offers = [];
        
    public DateTimeOffset LastUpdatedOnUtc { get; private set; }
        
    public BloggerId BloggerId { get; init;  }
        
    public Blogger? Blogger { get; init;  }
        
    public Content Content { get; private set; }
        
    public IReadOnlyCollection<Offer> Offers => this._offers.AsReadOnly();
        
    private Pact() { }

    private Pact(
        PactId id,
        BloggerId userId,
        Content content
    )
    {
        this.Id = id;
        this.BloggerId = userId;
        this.Content = content;
        this.LastUpdatedOnUtc = DateTimeOffset.UtcNow;
    }
        
    public static Result<Pact> Create(
        BloggerId userId,
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
        
    public Result Update(string content)
    {
        Result<Content> validationResult = Validate(content);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Pact>(validationResult.Error);
        }

        this.Content = validationResult.Value;
            
        return Result.Success();
    }
        
    private static Result<Content> Validate(string content)
    {
        Result<Content> contentResult = Content.Create(content);

        if (contentResult.IsFailure)
        {
            return Result.Failure<Content>(
                ValidationError.FromResults([contentResult])
            );
        }
            
        return contentResult.Value;
    }

    public bool HasOffer(Name offerName)
    {
        return this._offers.Exists(offer => 
            string.Equals(offer.Name.Value, offerName.Value, StringComparison.OrdinalIgnoreCase)
        );
    }
}
