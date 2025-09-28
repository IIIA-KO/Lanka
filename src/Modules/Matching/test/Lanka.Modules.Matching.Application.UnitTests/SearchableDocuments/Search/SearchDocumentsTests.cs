using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.SearchableDocuments.Search;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries;
using Lanka.Modules.Matching.Domain.Searches.SearchResults;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.Facets;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchHighlights;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchResultItems;
using NSubstitute;

namespace Lanka.Modules.Matching.Application.UnitTests.SearchableDocuments.Search;

public class SearchDocumentsTests
{
    private static SearchDocumentsQuery Query => new(
        "test query",
        EnableFuzzySearch: true,
        EnableSynonyms: true,
        FuzzyDistance: 0.8,
        ItemTypes: "1,2",
        NumericFilters: new Dictionary<string, object> { ["priority"] = 5 },
        FacetFilters: new Dictionary<string, IReadOnlyCollection<string>> 
        { 
            ["category"] = ["tech", "programming"] 
        },
        CreatedAfter: DateTimeOffset.UtcNow.AddDays(-30),
        CreatedBefore: DateTimeOffset.UtcNow,
        OnlyActive: true,
        Page: 1,
        Size: 10
    );

    private readonly ISearchService _searchServiceMock;
    private readonly SearchDocumentsQueryHandler _handler;

    public SearchDocumentsTests()
    {
        this._searchServiceMock = Substitute.For<ISearchService>();
        this._handler = new SearchDocumentsQueryHandler(this._searchServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSearchIsValid()
    {
        // Arrange
        SearchResult searchResult = CreateMockSearchResult();
        this._searchServiceMock
            .SearchAsync(Arg.Any<SearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        Result<SearchDocumentsResponse> result = await this._handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Results.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(100);
        result.Value.Page.Should().Be(1);
        result.Value.Size.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQueryIsInvalid()
    {
        // Arrange
        var invalidQuery = new SearchDocumentsQuery(
            string.Empty, // Invalid empty query
            Page: 1,
            Size: 10
        );

        // Act
        Result<SearchDocumentsResponse> result = await this._handler.Handle(invalidQuery, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await this._searchServiceMock.DidNotReceive()
            .SearchAsync(Arg.Any<SearchQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectSearchQueryToService()
    {
        // Arrange
        SearchResult searchResult = CreateMockSearchResult();
        this._searchServiceMock
            .SearchAsync(Arg.Any<SearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        await this._handler.Handle(Query, CancellationToken.None);

        // Assert
        await this._searchServiceMock.Received(1).SearchAsync(
            Arg.Is<SearchQuery>(q => 
                q.Text == Query.Query &&
                q.EnableFuzzySearch == Query.EnableFuzzySearch &&
                q.EnableSynonyms == Query.EnableSynonyms &&
                Math.Abs(q.FuzzyDistance - Query.FuzzyDistance) < 0.001 &&
                q.OnlyActive == Query.OnlyActive &&
                q.Page == Query.Page &&
                q.Size == Query.Size),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ShouldMapSearchResultCorrectly()
    {
        // Arrange
        SearchResult searchResult = CreateMockSearchResult();
        this._searchServiceMock
            .SearchAsync(Arg.Any<SearchQuery>(), Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        Result<SearchDocumentsResponse> result = await this._handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        SearchDocumentsResponse response = result.Value;
        
        response.Results.Should().HaveCount(2);
        response.TotalCount.Should().Be(searchResult.TotalCount);
        response.Page.Should().Be(searchResult.Page);
        response.Size.Should().Be(searchResult.Size);
        response.SearchDuration.Should().Be(searchResult.SearchDuration);
        response.Facets.Should().HaveCount(1);
        response.Facets.Should().ContainKey("category");
    }

    [Fact]
    public void SearchDocumentsQuery_ShouldGenerateCorrectCacheKey()
    {
        // Act
        string cacheKey = Query.CacheKey;

        // Assert
        cacheKey.Should().NotBeNullOrEmpty();
        cacheKey.Should().StartWith("search-docs-");
        cacheKey.Should().Contain("-fuzzy-True");
        cacheKey.Should().Contain("-syn-True");
        cacheKey.Should().Contain("-dist-0.8");
        cacheKey.Should().Contain("-types-1,2");
        cacheKey.Should().Contain("-active-True");
        cacheKey.Should().Contain("-page-1");
        cacheKey.Should().Contain("-size-10");
    }

    [Fact]
    public void SearchDocumentsQuery_ShouldHaveCorrectCacheExpiration()
    {
        // Act
        TimeSpan? expiration = Query.Expiration;

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(3));
    }

    private static SearchResult CreateMockSearchResult()
    {
        var highlights1 = new List<SearchHighlight>
        {
            SearchHighlight.Create("title", ["test <em>query</em> highlight"]).Value
        };

        var highlights2 = new List<SearchHighlight>
        {
            SearchHighlight.Create("content", ["content with <em>query</em> match"]).Value
        };

        var items = new List<SearchResultItem>
        {
            SearchResultItem.Create(
                Guid.NewGuid(),
                SearchableItemType.Blogger.ToString(),
                0.95,
                highlights1,
                new Dictionary<string, object> { ["category"] = "tech" }
            ).Value,
            SearchResultItem.Create(
                Guid.NewGuid(),
                SearchableItemType.Campaign.ToString(),
                0.87,
                highlights2,
                new Dictionary<string, object> { ["category"] = "programming" }
            ).Value
        };

        var facets = new Dictionary<string, IReadOnlyCollection<Facet>>
        {
            ["category"] = new List<Facet>
            {
                Facet.Create("tech", 50).Value,
                Facet.Create("programming", 30).Value
            }
        };

        return SearchResult.Create(
            items,
            100,
            1,
            10,
            TimeSpan.FromMilliseconds(150),
            facets
        ).Value;
    }
}
