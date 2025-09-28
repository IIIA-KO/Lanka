using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults.Facets;

public sealed record Facet
{
    public string Value { get; init; }
    public long Count { get; init; }

    private Facet(string value, long count)
    {
        this.Value = value;
        this.Count = count;
    }

    public static Result<Facet> Create(string value, long count)
    {
        Result validationResult = ValidateFacetValue(value, count);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Facet>(validationResult.Error);
        }

        return new Facet(value, Math.Max(0, count));
    }

    private static Result ValidateFacetValue(string value, long count)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FacetValueErrors.EmptyValue;
        }

        if (count < 0)
        {
            return FacetValueErrors.InvalidCount;
        }

        return Result.Success();
    }
}



