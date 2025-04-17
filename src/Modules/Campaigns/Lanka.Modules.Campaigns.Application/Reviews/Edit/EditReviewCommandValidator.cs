using FluentValidation;
using Lanka.Modules.Campaigns.Domain.Reviews.Ratings;

namespace Lanka.Modules.Campaigns.Application.Reviews.Edit;

internal sealed class EditReviewCommandValidator : AbstractValidator<EditReviewCommand>
{
    public EditReviewCommandValidator()
    {
        this.RuleFor(c => c.ReviewId)
            .NotNull()
            .NotEmpty()
            .WithMessage("ReviewId is required");

        this.RuleFor(c => c.Rating)
            .InclusiveBetween(Rating.MinValue, Rating.MaxValue)
            .WithMessage($"Rating must be between {Rating.MinValue} and {Rating.MaxValue}");

        this.RuleFor(c => c.Comment)
            .NotNull()
            .NotEmpty()
            .WithMessage("Comment is required");
    }
}
