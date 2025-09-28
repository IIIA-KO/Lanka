using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableDocuments;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.UnitTests.Abstractions;

namespace Lanka.Modules.Matching.UnitTests.SearchableDocuments;

public class SearchableDocumentTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidInput()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableDocumentData.Type,
            SearchableDocumentData.Title,
            SearchableDocumentData.Content,
            SearchableDocumentData.Tags,
            SearchableDocumentData.Metadata
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SourceEntityId.Should().Be(SearchableDocumentData.SourceEntityId);
        result.Value.Type.Should().Be(SearchableDocumentData.Type);
        result.Value.Title.Value.Should().Be(SearchableDocumentData.Title);
        result.Value.Content.Value.Should().Be(SearchableDocumentData.Content);
        result.Value.Tags.Should().NotBeEmpty();
        result.Value.Metadata.Should().NotBeEmpty();
        result.Value.IsActive.Should().BeTrue();
        result.Value.LastUpdated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WithMinimalInput()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.Title,
            SearchableDocumentData.Content,
            []
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Tags.Should().BeEmpty();
        result.Value.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldFilterEmptyTags()
    {
        // Arrange
        string[] tagsWithEmpty = ["tag1", "", "tag2", "   ", "tag3"];

        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.Title,
            SearchableDocumentData.Content,
            tagsWithEmpty
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().HaveCount(3);
        result.Value.Tags.Should().Contain("tag1");
        result.Value.Tags.Should().Contain("tag2");
        result.Value.Tags.Should().Contain("tag3");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.EmptyTitle,
            SearchableDocumentData.Content,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsWhitespace()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.WhitespaceTitle,
            SearchableDocumentData.Content,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenContentIsEmpty()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.Title,
            SearchableDocumentData.EmptyContent,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenContentIsWhitespace()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.Title,
            SearchableDocumentData.WhitespaceContent,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleTooLong()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.LongTitle,
            SearchableDocumentData.Content,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenContentTooLong()
    {
        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            SearchableDocumentData.Title,
            SearchableDocumentData.LongContent,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldTrimTitleAndContent()
    {
        // Arrange
        string titleWithSpaces = "  " + SearchableDocumentData.Title + "  ";
        string contentWithSpaces = "  " + SearchableDocumentData.Content + "  ";

        // Act
        Result<SearchableDocument> result = SearchableDocument.Create(
            SearchableDocumentData.SourceEntityId,
            SearchableItemType.Blogger,
            titleWithSpaces,
            contentWithSpaces,
            SearchableDocumentData.Tags
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Value.Should().Be(SearchableDocumentData.Title);
        result.Value.Content.Value.Should().Be(SearchableDocumentData.Content);
    }
}
