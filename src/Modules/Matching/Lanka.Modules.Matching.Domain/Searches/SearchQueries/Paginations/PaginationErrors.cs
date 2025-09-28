using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.Paginations;

public static class PaginationErrors
{
    public static Error InvalidPage =>
        Error.Validation("Pagination.InvalidPage", "Page must be greater than 0.");

    public static Error InvalidSize =>
        Error.Validation("Pagination.InvalidSize", "Size must be between 1 and 100.");
}
