using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Application.Index.Update;
using Lanka.Modules.Matching.Domain.SearchableItems;
using NSubstitute;

namespace Lanka.Modules.Matching.Application.UnitTests.SearchableDocuments.Update;

public class UpdateSearchableDocumentContentTests
{
    private static UpdateSearchableDocumentContentCommand Command => new(
        SearchableDocumentData.SourceEntityId,
        SearchableDocumentData.Type,
        SearchableDocumentData.UpdatedTitle,
        SearchableDocumentData.UpdatedContent,
        SearchableDocumentData.UpdatedTags,
        SearchableDocumentData.Metadata
    );

    private readonly ISearchIndexService _searchIndexServiceMock;
    private readonly UpdateSearchableDocumentContentCommandHandler _handler;

    public UpdateSearchableDocumentContentTests()
    {
        this._searchIndexServiceMock = Substitute.For<ISearchIndexService>();
        this._handler = new UpdateSearchableDocumentContentCommandHandler(this._searchIndexServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUpdateIsValid()
    {
        // Arrange
        this._searchIndexServiceMock
            .UpdateDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        await this._searchIndexServiceMock.Received(1)
            .UpdateDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDocumentCreationFails()
    {
        // Arrange
        UpdateSearchableDocumentContentCommand invalidCommand = new(
            SearchableDocumentData.SourceEntityId,
            SearchableDocumentData.Type,
            SearchableDocumentData.EmptyTitle,
            SearchableDocumentData.EmptyContent, // Both empty - invalid
            SearchableDocumentData.UpdatedTags,
            SearchableDocumentData.Metadata
        );

        // Act
        Result result = await this._handler.Handle(invalidCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        await this._searchIndexServiceMock.DidNotReceive()
            .UpdateDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        var updateError = Error.Failure("UpdateError", "Failed to update document");
        this._searchIndexServiceMock
            .UpdateDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(updateError));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(updateError);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateWithCorrectData()
    {
        // Arrange
        SearchDocument? capturedDocument = null;
        this._searchIndexServiceMock
            .UpdateDocumentAsync(Arg.Do<SearchDocument>(d => capturedDocument = d), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.SourceEntityId.Should().Be(Command.SourceEntityId);
        capturedDocument.Type.Should().Be(Command.Type);
        capturedDocument.Title.Should().Be(Command.Title);
        capturedDocument.Content.Should().Be(Command.Content);
    }
}
