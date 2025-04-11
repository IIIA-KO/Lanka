using FluentValidation;
using Lanka.Modules.Campaigns.Domain.Reviews.Ratings;

namespace Lanka.Modules.Campaigns.Application.Reviews.Create;

internal sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        this.RuleFor(c => c.Rating)
            .InclusiveBetween(Rating.MinValue, Rating.MaxValue)
            .WithMessage($"Rating must be between {Rating.MinValue} and {Rating.MaxValue}");

        this.RuleFor(c => c.Comment)
            .NotNull()
            .NotEmpty()
            .WithMessage("Comment is required");
    }
}
