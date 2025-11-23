using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries;
using Lanka.Modules.Matching.Domain.Searches.SearchResults;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchHighlights;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchResultItems;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Documents;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Matching.Infrastructure.Elasticsearch.Services;

internal sealed class ElasticSearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchService> _logger;
    private readonly ElasticSearchOptions _options;

    public ElasticSearchService(
        ElasticsearchClient client,
        ILogger<ElasticSearchService> logger,
        IOptions<ElasticSearchOptions> options
    )
    {
        this._client = client;
        this._logger = logger;
        this._options = options.Value;
    }

    public async Task<SearchResult> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var searchRequest = new SearchRequest(this._options.DefaultIndex)
            {
                Query = BuildQuery(query),
                Size = query.Size,
                From = (query.Page - 1) * query.Size,
                Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
                    {
                        { "title", new HighlightField { FragmentSize = 150, NumberOfFragments = 3 } },
                        { "content", new HighlightField { FragmentSize = 150, NumberOfFragments = 3 } },
                        { "tags", new HighlightField { FragmentSize = 100, NumberOfFragments = 2 } }
                    },
                    PreTags = ["<mark>"],
                    PostTags = ["</mark>"]
                }
            };

            SearchResponse<SearchableDocumentElastic> response =
                await this._client.SearchAsync<SearchableDocumentElastic>(searchRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogError("Elasticsearch search failed: {Error}", response.DebugInformation);
                return SearchResult.Empty(query.Page, query.Size);
            }

            var searchResultItems = response.Documents
                .Zip(response.Hits, this.MapToSearchResultItem)
                .Where(item => item != null)
                .ToList();

            Result<SearchResult> searchResult = SearchResult.Create(
                searchResultItems!,
                response.Total,
                query.Page,
                query.Size,
                stopwatch.Elapsed
            );

            return searchResult.IsSuccess ? searchResult.Value : SearchResult.Empty(query.Page, query.Size);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error performing Elasticsearch search");
            return SearchResult.Empty(query.Page, query.Size);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<SearchResult> SearchSimilarAsync(
        Guid sourceEntityId,
        SearchableItemType sourceType,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var searchRequest = new SearchRequest(this._options.DefaultIndex)
            {
                Query = this.BuildMoreLikeThisQuery(sourceEntityId, sourceType, query),
                Size = query.Size,
                From = (query.Page - 1) * query.Size
            };

            SearchResponse<SearchableDocumentElastic> response =
                await this._client.SearchAsync<SearchableDocumentElastic>(searchRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogError("Elasticsearch similar search failed: {Error}", response.DebugInformation);
                return SearchResult.Empty(query.Page, query.Size);
            }

            var searchResultItems = response.Documents
                .Zip(response.Hits, this.MapToSearchResultItem)
                .Where(item => item != null)
                .ToList();

            Result<SearchResult> searchResult = SearchResult.Create(
                searchResultItems!,
                response.Total,
                query.Page,
                query.Size,
                stopwatch.Elapsed
            );

            return searchResult.IsSuccess ? searchResult.Value : SearchResult.Empty(query.Page, query.Size);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error performing Elasticsearch similar search");
            return SearchResult.Empty(query.Page, query.Size);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<IReadOnlyCollection<string>> GetSearchSuggestionsAsync(
        string partialQuery,
        SearchableItemType? itemType = null,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
            {
                return [];
            }

            return await this.GetFallbackSuggestions(partialQuery, itemType, maxSuggestions, cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting search suggestions from Elasticsearch");
            return [];
        }
    }

    private async Task<IReadOnlyCollection<string>> GetFallbackSuggestions(
        string partialQuery,
        SearchableItemType? itemType,
        int maxSuggestions,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new BoolQuery
            {
                Should =
                [
                    new PrefixQuery { Field = "title", Value = partialQuery },
                    new WildcardQuery { Field = "title", Value = $"*{partialQuery}*" }
                ],
                MinimumShouldMatch = 1
            };

            if (itemType.HasValue)
            {
                query.Filter =
                [
                    new TermQuery { Field = "type", Value = itemType.Value.ToString() }
                ];
            }

            var searchRequest = new SearchRequest(this._options.DefaultIndex)
            {
                Query = query,
                Size = maxSuggestions
            };

            SearchResponse<SearchableDocumentElastic> response =
                await this._client.SearchAsync<SearchableDocumentElastic>(searchRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                return [];
            }

            return response.Documents
                .Select(doc => doc.Title)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxSuggestions)
                .ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting fallback search suggestions");
            return [];
        }
    }

    private static Query BuildQuery(SearchQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Text))
        {
            return new MatchAllQuery();
        }
        
        var multiMatchQuery = new MultiMatchQuery
        {
            Query = query.Text,
            Fields = new Field[] { new("title^2"), new("content"), new("tags") }, // Boost title matches
            Type = TextQueryType.BestFields
        };

        if (query.FuzzyDistance > 0)
        {
            multiMatchQuery.Fuzziness =
                new Fuzziness(query.FuzzyDistance.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return multiMatchQuery;
    }

    private Query BuildMoreLikeThisQuery(Guid sourceEntityId, SearchableItemType sourceType, SearchQuery query)
    {
        var moreLikeThisQuery = new MoreLikeThisQuery
        {
            Fields = new Field[] { "title", "content", "tags" },
            Like =
            [
                new LikeDocument
                {
                    Index = this._options.DefaultIndex,
                    Id = sourceEntityId.ToString()
                }
            ],
            MinTermFreq = 1,
            MaxQueryTerms = 12,
            MinDocFreq = 1,
            MaxDocFreq = null,
            MinWordLength = 2,
            MaxWordLength = null,
            StopWords = null,
            Analyzer = "icu_analyzer",
            MinimumShouldMatch = "30%",
            BoostTerms = 1.0f,
            Include = false, // Don't include the source document
            Boost = 1.0f
        };

        var boolQuery = new BoolQuery
        {
            Must = [moreLikeThisQuery],
            Filter = [],
            MustNot =
            [
                new TermQuery { Field = "sourceEntityId", Value = sourceEntityId.ToString() }
            ]
        };

        if (query.ItemTypes.Any())
        {
            var typeFilters = query.ItemTypes
                .Select(type => (Query)new TermQuery { Field = "type", Value = type.ToString() })
                .ToList();

            if (typeFilters.Any())
            {
                boolQuery.Filter = boolQuery.Filter.Concat(typeFilters).ToList();
            }
        }
        else
        {
            boolQuery.Filter = boolQuery.Filter.Concat(
            [
                new TermQuery { Field = "type", Value = sourceType.ToString() }
            ]).ToList();
        }

        if (query.OnlyActive)
        {
            boolQuery.Filter = boolQuery.Filter.Concat(
            [
                new TermQuery { Field = "isActive", Value = "true" }
            ]).ToList();
        }

        if (!query.CreatedAfter.HasValue && !query.CreatedBefore.HasValue)
        {
            return boolQuery;
        }

        var rangeQuery = new DateRangeQuery("lastUpdated");

        if (query.CreatedAfter.HasValue)
        {
            rangeQuery.Gte =
                DateMath.Anchored(query.CreatedAfter.Value.DateTime);
        }

        if (query.CreatedBefore.HasValue)
        {
            rangeQuery.Lte = DateMath.Anchored(query.CreatedBefore.Value.DateTime);
        }

        boolQuery.Filter = boolQuery.Filter.Concat([rangeQuery]).ToList();

        return boolQuery;
    }

    private SearchResultItem? MapToSearchResultItem(
        SearchableDocumentElastic doc,
        Hit<SearchableDocumentElastic> hit
    )
    {
        try
        {
            var highlights = new List<SearchHighlight>();

            if (hit.Highlight != null && hit.Highlight.Any())
            {
                foreach (KeyValuePair<string, IReadOnlyCollection<string>> highlightField in hit.Highlight)
                {
                    string fieldName = highlightField.Key;
                    var fragments = highlightField.Value.ToList();

                    if (!fragments.Any())
                    {
                        continue;
                    }

                    Result<SearchHighlight> highlightResult = SearchHighlight.Create(fieldName, fragments);

                    if (highlightResult.IsSuccess)
                    {
                        highlights.Add(highlightResult.Value);
                    }
                }
            }

            Result<SearchResultItem> resultItem = SearchResultItem.Create(
                doc.SourceEntityId,
                doc.Type,
                hit.Score ?? 0.0,
                highlights,
                doc.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );

            return resultItem.IsSuccess ? resultItem.Value : null;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error mapping search result item for document {DocumentId}", doc.Id);
            return null;
        }
    }
}
