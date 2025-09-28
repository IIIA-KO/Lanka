using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.SearchableDocuments.Update;
using Lanka.Modules.Matching.Domain.SearchableDocuments;
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
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        await this._searchIndexServiceMock.Received(1)
            .RemoveDocumentsBySourceEntityAsync(
                Command.SourceEntityId, 
                Command.Type, 
                Arg.Any<CancellationToken>());
                
        await this._searchIndexServiceMock.Received(1)
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRemovalFails()
    {
        // Arrange
        var removeError = Error.Failure("RemoveError", "Failed to remove document");
        this._searchIndexServiceMock
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure(removeError));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(removeError);
        
        await this._searchIndexServiceMock.DidNotReceive()
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDocumentCreationFails()
    {
        // Arrange
        this._searchIndexServiceMock
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var invalidCommand = new UpdateSearchableDocumentContentCommand(
            SearchableDocumentData.SourceEntityId,
            SearchableDocumentData.Type,
            SearchableDocumentData.EmptyTitle, // Invalid title
            SearchableDocumentData.UpdatedContent,
            SearchableDocumentData.UpdatedTags,
            SearchableDocumentData.Metadata
        );

        // Act
        Result result = await this._handler.Handle(invalidCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        await this._searchIndexServiceMock.DidNotReceive()
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIndexingFails()
    {
        // Arrange
        this._searchIndexServiceMock
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var indexError = Error.Failure("IndexError", "Failed to index document");
        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(indexError));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(indexError);
    }

    [Fact]
    public async Task Handle_ShouldProcessInCorrectOrder()
    {
        // Arrange
        this._searchIndexServiceMock
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        Received.InOrder(async () =>
        {
            await this._searchIndexServiceMock.RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>());
                
            await this._searchIndexServiceMock.IndexDocumentAsync(
                Arg.Any<SearchableDocument>(), 
                Arg.Any<CancellationToken>());
        });
    }
}
