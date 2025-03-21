using Lanka.Common.Contracts.Monies;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns.Descriptions;
using Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;
using Lanka.Modules.Campaigns.Domain.Offers;
using Name = Lanka.Modules.Campaigns.Domain.Campaigns.Names.Name;

namespace Lanka.Modules.Campaigns.Domain.Campaigns;

public class Campaign : Entity<CampaignId>
{
    public static readonly CampaignStatus[] activeCampaignStatuses =
    [
        CampaignStatus.Pending,
        CampaignStatus.Confirmed,
        CampaignStatus.Done
    ];
        
    public Name Name { get; private set; }
        
    public Description Description { get; private set; }

    public Money Price { get; init; }
        
    public OfferId OfferId { get; init; }
        
    public Offer? Offer { get; init; }
        
    public BloggerId ClientId { get; init; }
        
    public Blogger? Client { get; init; }
        
    public BloggerId CreatorId { get; init; }
        
    public Blogger? Creator { get; init; }
        
    public CampaignStatus Status { get; private set; }

    public DateTimeOffset ScheduledOnUtc { get; init; }
        
    public DateTimeOffset PendedOnUtc { get; init; }

    public DateTimeOffset? ConfirmedOnUtc { get; private set; }

    public DateTimeOffset? RejectedOnUtc { get; private set; }

    public DateTimeOffset? CancelledOnUtc { get; private set; }

    public DateTimeOffset? DoneOnUtc { get; private set; }

    public DateTimeOffset? CompletedOnUtc { get; private set; }

    private Campaign() {}

    private Campaign(
        CampaignId id,
        Name name,
        Description description,
        Money price,
        DateTimeOffset scheduledOnUtc,
        OfferId offerId,
        BloggerId clientId,
        BloggerId creatorId,
        CampaignStatus status,
        DateTimeOffset pendedOnUtc
    ) : base(id)
    {
        this.Name = name;
        this.Description = description;
        this.Price = price;
        this.ScheduledOnUtc = scheduledOnUtc;
        this.OfferId = offerId;
        this.ClientId = clientId;
        this.CreatorId = creatorId;
        this.Status = status;
        this.PendedOnUtc = pendedOnUtc;
    }
        
    public static Result<Campaign> Pend(
        string name,
        string description,
        DateTimeOffset scheduledOnUtc,
        Offer offer,
        BloggerId clientId,
        BloggerId creatorId,
        DateTimeOffset utcNow
    )
    {
        Result<(Name, Description)> validationResult = Validate(name, description);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Campaign>(validationResult.Error);
        }
            
        (Name nm, Description desc) = validationResult.Value;
            
        if (clientId == creatorId)
        {
            return Result.Failure<Campaign>(CampaignErrors.SameUser);
        }

        if (scheduledOnUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<Campaign>(CampaignErrors.InvalidTime);
        }

        if (offer.Price.Amount < 0)
        {
            return Result.Failure<Campaign>(MoneyErrors.NegativeAmount);
        }
            
        var campaign = new Campaign(
            CampaignId.New(),
            nm,
            desc,
            offer.Price,
            utcNow,
            offer.Id,
            clientId,
            creatorId,
            CampaignStatus.Pending,
            utcNow
        );
            
        campaign.RaiseDomainEvent(new CampaignPendedDomainEvent(campaign.Id));

        offer.UpdateOffer(utcNow);
            
        return campaign;
    }

    private static Result<(Name, Description)> Validate(string name, string description)
    {
        Result<Name> nameResult = Name.Create(name);
        Result<Description> descriptionResult = Description.Create(description);

        if (nameResult.IsFailure || descriptionResult.IsFailure)
        {
            return Result.Failure<(Name, Description)>(
                ValidationError.FromResults([nameResult, descriptionResult])
            );
        }
            
        return (nameResult.Value, descriptionResult.Value);
    }
        
    public Result Confirm(DateTimeOffset utcNow)
    {
        if (this.Status != CampaignStatus.Pending)
        {
            return Result.Failure(CampaignErrors.NotPending);
        }

        this.Status = CampaignStatus.Confirmed;
        this.ConfirmedOnUtc = utcNow;

        this.RaiseDomainEvent(new CampaignConfirmedDomainEvent(this.Id));

        return Result.Success();
    }

    public Result Reject(DateTimeOffset utcNow)
    {
        if (this.Status != CampaignStatus.Pending)
        {
            return Result.Failure(CampaignErrors.NotPending);
        }

        this.Status = CampaignStatus.Rejected;
        this.RejectedOnUtc = utcNow;

        this.RaiseDomainEvent(new CampaignRejectedDomainEvent(this.Id));
        return Result.Success();
    }

    public Result MarkAsDone(DateTimeOffset utcNow)
    {
        if (this.Status != CampaignStatus.Confirmed)
        {
            return Result.Failure(CampaignErrors.NotConfirmed);
        }

        this.Status = CampaignStatus.Done;
        this.DoneOnUtc = utcNow;

        this.RaiseDomainEvent(new CampaignMarkedAsDoneDomainEvent(this.Id));

        return Result.Success();
    }

    public Result Complete(DateTimeOffset utcNow)
    {
        if (this.Status != CampaignStatus.Done)
        {
            return Result.Failure(CampaignErrors.NotDone);
        }

        this.Status = CampaignStatus.Completed;
        this.CompletedOnUtc = utcNow;

        this.RaiseDomainEvent(new CampaignCompletedDomainEvent(this.Id));
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset utcNow)
    {
        if (this.Status != CampaignStatus.Confirmed)
        {
            return Result.Failure(CampaignErrors.NotConfirmed);
        }

        if (utcNow > this.ScheduledOnUtc)
        {
            return Result.Failure(CampaignErrors.AlreadyStarted);
        }

        this.Status = CampaignStatus.Cancelled;
        this.CancelledOnUtc = utcNow;

        this.RaiseDomainEvent(new CampaignCancelledDomainEvent(this.Id));
        return Result.Success();
    }
}
