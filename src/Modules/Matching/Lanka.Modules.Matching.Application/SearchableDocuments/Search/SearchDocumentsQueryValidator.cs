using FluentValidation;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.FuzzyDistances;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.Paginations;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.SearchTexts;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Search;

internal sealed class SearchDocumentsQueryValidator : AbstractValidator<SearchDocumentsQuery>
{
    public SearchDocumentsQueryValidator()
    {
        this
            .RuleFor(q => q.Query)
            .NotNull()
            .NotEmpty()
            .WithMessage("Query is required.")
            .MaximumLength(SearchText.MaxLength)
            .WithMessage($"Query must not exceed {SearchText.MaxLength} characters.");
        
        this
            .RuleFor(q => q.FuzzyDistance)
            .InclusiveBetween(FuzzyDistance.Min, FuzzyDistance.Max)
            .WithMessage($"Fuzzy distance must be between {FuzzyDistance.Min} and {FuzzyDistance.Max}.");

        this
            .RuleFor(q => q.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        this
            .RuleFor(q => q.Size)
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
