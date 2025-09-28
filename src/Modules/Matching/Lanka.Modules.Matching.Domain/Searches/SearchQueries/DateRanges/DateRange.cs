using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.DateRanges;

public sealed record DateRange
{
    public DateTimeOffset? CreatedAfter { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }

    private DateRange(DateTimeOffset? createdAfter, DateTimeOffset? createdBefore)
    {
        this.CreatedAfter = createdAfter;
        this.CreatedBefore = createdBefore;
    }

    public static Result<DateRange> Create(DateTimeOffset? createdAfter, DateTimeOffset? createdBefore)
    {
        Result validationResult = ValidateDateRange(createdAfter, createdBefore);

        if (validationResult.IsFailure)
        {
            return Result.Failure<DateRange>(validationResult.Error);
        }

        return new DateRange(createdAfter, createdBefore);
    }

    private static Result ValidateDateRange(DateTimeOffset? createdAfter, DateTimeOffset? createdBefore)
    {
        if (createdAfter.HasValue && createdBefore.HasValue && createdAfter > createdBefore)
        {
            return DateRangeErrors.Invalid;
        }

        return Result.Success();
    }
}
