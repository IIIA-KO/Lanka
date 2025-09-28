using FluentAssertions;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.UnitTests.Abstractions;

namespace Lanka.Modules.Matching.UnitTests.SearchableItems;

public class SearchableItemTests : BaseTest
{
    [Fact]
    public void Constructor_ShouldCreateSearchableItem()
    {
        // Act
        var searchableItem = new SearchableItem(
            SearchableItemData.Id,
            SearchableItemData.Type,
            SearchableItemData.Title,
            SearchableItemData.Content,
            SearchableItemData.Tags
        );

        // Assert
        searchableItem.Should().NotBeNull();
        searchableItem.Id.Should().Be(SearchableItemData.Id);
        searchableItem.Type.Should().Be(SearchableItemData.Type);
        searchableItem.Title.Should().Be(SearchableItemData.Title);
        searchableItem.Content.Should().Be(SearchableItemData.Content);
        searchableItem.Tags.Should().BeEquivalentTo(SearchableItemData.Tags);
        searchableItem.IsActive.Should().BeTrue();
        searchableItem.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var searchableItem = new SearchableItem(
            SearchableItemData.Id,
            SearchableItemData.Type,
            SearchableItemData.Title,
            SearchableItemData.Content,
            SearchableItemData.Tags
        );
        DateTime originalLastUpdated = searchableItem.LastUpdated;

        // Act
        searchableItem.Deactivate();

        // Assert
        searchableItem.IsActive.Should().BeFalse();
        searchableItem.LastUpdated.Should().BeAfter(originalLastUpdated);
    }

    [Fact]
    public void UpdateContent_ShouldUpdateAllFields()
    {
        // Arrange
        var searchableItem = new SearchableItem(
            SearchableItemData.Id,
            SearchableItemData.Type,
            SearchableItemData.Title,
            SearchableItemData.Content,
            SearchableItemData.Tags
        );
        DateTime originalLastUpdated = searchableItem.LastUpdated;

        // Act
        searchableItem.UpdateContent(
            SearchableItemData.UpdatedTitle,
            SearchableItemData.UpdatedContent,
            SearchableItemData.UpdatedTags
        );

        // Assert
        searchableItem.Title.Should().Be(SearchableItemData.UpdatedTitle);
        searchableItem.Content.Should().Be(SearchableItemData.UpdatedContent);
        searchableItem.Tags.Should().BeEquivalentTo(SearchableItemData.UpdatedTags);
        searchableItem.LastUpdated.Should().BeAfter(originalLastUpdated);
    }

    [Fact]
    public void UpdateContent_ShouldPreserveIdAndType()
    {
        // Arrange
        var searchableItem = new SearchableItem(
            SearchableItemData.Id,
            SearchableItemData.Type,
            SearchableItemData.Title,
            SearchableItemData.Content,
            SearchableItemData.Tags
        );

        // Act
        searchableItem.UpdateContent(
            SearchableItemData.UpdatedTitle,
            SearchableItemData.UpdatedContent,
            SearchableItemData.UpdatedTags
        );

        // Assert
        searchableItem.Id.Should().Be(SearchableItemData.Id);
        searchableItem.Type.Should().Be(SearchableItemData.Type);
        searchableItem.IsActive.Should().BeTrue(); // Should remain active
    }

    [Fact]
    public void UpdateContent_WithEmptyTags_ShouldSetEmptyTagsList()
    {
        // Arrange
        var searchableItem = new SearchableItem(
            SearchableItemData.Id,
            SearchableItemData.Type,
            SearchableItemData.Title,
            SearchableItemData.Content,
            SearchableItemData.Tags
        );

        // Act
        searchableItem.UpdateContent(
            SearchableItemData.UpdatedTitle,
            SearchableItemData.UpdatedContent,
            []
        );

        // Assert
        searchableItem.Tags.Should().BeEmpty();
    }
}
