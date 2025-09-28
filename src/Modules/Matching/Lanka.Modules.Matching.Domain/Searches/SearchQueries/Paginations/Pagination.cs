using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.Paginations;

public sealed record Pagination
{
    public const int MinPage = 1;
    public const int MinSize = 1;
    public const int MaxSize = 100;

    public int Page { get; init; }
    public int Size { get; init; }

    private Pagination(int page, int size)
    {
        this.Page = page;
        this.Size = size;
    }

    public static Result<Pagination> Create(int page, int size)
    {
        Result validationResult = ValidatePagination(page, size);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Pagination>(validationResult.Error);
        }

        return new Pagination(page, size);
    }

    private static Result ValidatePagination(int page, int size)
    {
        if (page < MinPage)
        {
            return PaginationErrors.InvalidPage;
        }

        if (size is < MinSize or > MaxSize)
        {
            return PaginationErrors.InvalidSize;
        }

        return Result.Success();
    }
}
