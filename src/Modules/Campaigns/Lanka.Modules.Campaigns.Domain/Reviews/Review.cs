using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Reviews.Comments;
using Lanka.Modules.Campaigns.Domain.Reviews.DomainEvents;
using Lanka.Modules.Campaigns.Domain.Reviews.Ratings;

namespace Lanka.Modules.Campaigns.Domain.Reviews;

public sealed class Review : Entity<ReviewId>
{
    private Review() { }

    private Review(
        ReviewId reviewId,
        BloggerId campaignClientId,
        BloggerId campaignCreatorId,
        OfferId campaignOfferId,
        CampaignId campaignId,
        Comment comment,
        Rating rating,
        DateTimeOffset createdOnUtc
    ) : base(reviewId)
    {
        this.Id = reviewId;
        this.ClientId = campaignClientId;
        this.CreatorId = campaignCreatorId;
        this.OfferId = campaignOfferId;
        this.CampaignId = campaignId;
        this.Comment = comment;
        this.Rating = rating;
        this.CreatedOnUtc = createdOnUtc;
    }

    public BloggerId ClientId { get; init; }

    public BloggerId CreatorId { get; init; }

    public OfferId OfferId { get; init; }

    public CampaignId CampaignId { get; init; }

    public Rating Rating { get; private set; }

    public Comment Comment { get; private set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }

    public static Result<Review> Create(
        Campaign campaign,
        int rating,
        string comment,
        DateTimeOffset createdOnUtc
    )
    {
        Result<(Rating, Comment)> validationResult = Validate(rating, comment);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Review>(validationResult.Error);
        }

        (Rating _rating, Comment _comment) = validationResult.Value;

        if (campaign.Status != CampaignStatus.Completed)
        {
            return Result.Failure<Review>(ReviewErrors.NotEligible);
        }

        var review = new Review(
            ReviewId.New(),
            campaign.ClientId,
            campaign.CreatorId,
            campaign.OfferId,
            campaign.Id,
            _comment,
            _rating,
            createdOnUtc
        );

        review.RaiseDomainEvent(new ReviewCreatedDomainEvent(review.Id));

        return review;
    }

    public Result Update(int rating, string comment)
    {
        Result<(Rating, Comment)> validationResult = Validate(rating, comment);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        (this.Rating, this.Comment) = validationResult.Value;

        this.RaiseDomainEvent(new ReviewUpdatedDomainEvent(this.Id));

        return Result.Success();
    }

    private static Result<(Rating, Comment)> Validate(int rating, string comment)
    {
        Result<Rating> ratingResult = Rating.Create(rating);
        Result<Comment> commentResult = Comment.Create(comment);

        return new ValidationBuilder()
            .Add(ratingResult)
            .Add(commentResult)
            .Build(() => (ratingResult.Value, commentResult.Value));
    }
    
    public void Delete()
    {
        this.RaiseDomainEvent(new ReviewDeletedDomainEvent(this.Id));
    }
}
