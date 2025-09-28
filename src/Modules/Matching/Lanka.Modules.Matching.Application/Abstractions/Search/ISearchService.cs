using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries;
using Lanka.Modules.Matching.Domain.Searches.SearchResults;

namespace Lanka.Modules.Matching.Application.Abstractions.Search;

public interface ISearchService
{
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    
    Task<SearchResult> SearchSimilarAsync(
        Guid sourceEntityId,
        SearchableItemType sourceType,
        SearchQuery query,
        CancellationToken cancellationToken = default
    );
    
    Task<IReadOnlyCollection<string>> GetSearchSuggestionsAsync(
        string partialQuery,
        SearchableItemType? itemType = null,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default
    );
}
