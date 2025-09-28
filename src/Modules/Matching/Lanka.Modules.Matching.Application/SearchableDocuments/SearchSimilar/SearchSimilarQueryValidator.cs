using FluentValidation;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.Paginations;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.SearchSimilar;

internal sealed class SearchSimilarQueryValidator : AbstractValidator<SearchSimilarQuery>
{
    public SearchSimilarQueryValidator()
    {
        this.RuleFor(q => q.SourceItemId)
            .NotEmpty()
            .WithMessage("Source item id is required.");

        this.RuleFor(q => q.SourceType)
            .IsInEnum()
            .WithMessage("Invalid source type.");

        this.RuleFor(q => q.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        this.RuleFor(q => q.Size)
            .InclusiveBetween(Pagination.MinSize, Pagination.MaxSize)
            .WithMessage($"Page size must be between {Pagination.MinSize} and {Pagination.MaxSize}.");

        this.RuleFor(q => q.CreatedAfter)
            .LessThan(q => q.CreatedBefore)
            .WithMessage("CreatedAfter must be before CreatedBefore.")
            .When(q => q.CreatedAfter.HasValue && q.CreatedBefore.HasValue);

        this.RuleFor(q => q.CreatedBefore)
            .GreaterThan(q => q.CreatedAfter)
            .WithMessage("CreatedBefore must be after CreatedAfter.")
            .When(q => q.CreatedAfter.HasValue && q.CreatedBefore.HasValue);
    }
}
