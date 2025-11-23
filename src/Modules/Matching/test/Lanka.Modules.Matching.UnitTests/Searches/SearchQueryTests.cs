using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries;
using Lanka.Modules.Matching.UnitTests.Abstractions;

namespace Lanka.Modules.Matching.UnitTests.Searches;

public class SearchQueryTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnSuccess_WithValidParameters()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(
            SearchQueryData.Text,
            true,
            true,
            SearchQueryData.ValidFuzzyDistance,
            SearchQueryData.ItemTypes,
            SearchQueryData.NumericFilters,
            SearchQueryData.FacetFilters,
            SearchQueryData.CreatedAfter,
            SearchQueryData.CreatedBefore,
            true,
            SearchQueryData.ValidPage,
            SearchQueryData.ValidSize
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Text.Should().Be(SearchQueryData.Text);
        result.Value.EnableFuzzySearch.Should().BeTrue();
        result.Value.EnableSynonyms.Should().BeTrue();
        result.Value.FuzzyDistance.Should().Be(SearchQueryData.ValidFuzzyDistance);
        result.Value.OnlyActive.Should().BeTrue();
        result.Value.Page.Should().Be(SearchQueryData.ValidPage);
        result.Value.Size.Should().Be(SearchQueryData.ValidSize);
        result.Value.CreatedAfter.Should().Be(SearchQueryData.CreatedAfter);
        result.Value.CreatedBefore.Should().Be(SearchQueryData.CreatedBefore);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WithMinimalParameters()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(SearchQueryData.Text);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Text.Should().Be(SearchQueryData.Text);
        result.Value.EnableFuzzySearch.Should().BeTrue();
        result.Value.EnableSynonyms.Should().BeTrue();
        result.Value.FuzzyDistance.Should().Be(0.8);
        result.Value.OnlyActive.Should().BeTrue();
        result.Value.Page.Should().Be(1);
        result.Value.Size.Should().Be(20);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTextIsEmpty()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(SearchQueryData.EmptyText);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTextIsWhitespace()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(SearchQueryData.WhitespaceText);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenFuzzyDistanceIsInvalid()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(
            SearchQueryData.Text,
            fuzzyDistance: SearchQueryData.InvalidFuzzyDistanceLow
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPageIsInvalid()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(
            SearchQueryData.Text,
            page: SearchQueryData.InvalidPage
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenSizeIsInvalid()
    {
        // Act
        Result<SearchQuery> result = SearchQuery.Create(
            SearchQueryData.Text,
            size: SearchQueryData.InvalidSize
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Simple_ShouldCreateBasicQuery()
    {
        // Act
        var query = SearchQuery.Simple(SearchQueryData.Text);

        // Assert
        query.Should().NotBeNull();
        query.Text.Should().Be(SearchQueryData.Text);
        query.EnableFuzzySearch.Should().BeTrue();
        query.EnableSynonyms.Should().BeTrue();
        query.Page.Should().Be(1);
        query.Size.Should().Be(20);
    }

    [Fact]
    public void WithFilters_ShouldCreateQueryWithFilters()
    {
        // Act
        var query = SearchQuery.WithFilters(
            SearchQueryData.Text,
            SearchQueryData.ItemTypes,
            SearchQueryData.NumericFilters,
            SearchQueryData.FacetFilters
        );

        // Assert
        query.Should().NotBeNull();
        query.Text.Should().Be(SearchQueryData.Text);
        query.NumericFilters.Should().BeEquivalentTo(SearchQueryData.NumericFilters);
        query.FacetFilters.Should().BeEquivalentTo(SearchQueryData.FacetFilters);
    }

    [Fact]
    public void WithDateRange_ShouldCreateQueryWithDateRange()
    {
        // Act
        var query = SearchQuery.WithDateRange(
            SearchQueryData.Text,
            SearchQueryData.CreatedAfter,
            SearchQueryData.CreatedBefore
        );

        // Assert
        query.Should().NotBeNull();
        query.Text.Should().Be(SearchQueryData.Text);
        query.CreatedAfter.Should().Be(SearchQueryData.CreatedAfter);
        query.CreatedBefore.Should().Be(SearchQueryData.CreatedBefore);
    }

    [Fact]
    public void WithItemTypes_ShouldUpdateItemTypes()
    {
        // Arrange
        var originalQuery = SearchQuery.Simple(SearchQueryData.Text);
        SearchableItemType[] itemTypes = [SearchableItemType.Blogger, SearchableItemType.Campaign];

        // Act
        SearchQuery updatedQuery = originalQuery.WithItemTypes(itemTypes);

        // Assert
        updatedQuery.Should().NotBeNull();
        updatedQuery.ItemTypes.Should().BeEquivalentTo(itemTypes);
        updatedQuery.Text.Should().Be(originalQuery.Text);
    }

    [Fact]
    public void WithPagination_ShouldUpdatePagination()
    {
        // Arrange
        var originalQuery = SearchQuery.Simple(SearchQueryData.Text);
        int newPage = 5;
        int newSize = 50;

        // Act
        SearchQuery updatedQuery = originalQuery.WithPagination(newPage, newSize);

        // Assert
        updatedQuery.Should().NotBeNull();
        updatedQuery.Page.Should().Be(newPage);
        updatedQuery.Size.Should().Be(newSize);
        updatedQuery.Text.Should().Be(originalQuery.Text);
    }
}
