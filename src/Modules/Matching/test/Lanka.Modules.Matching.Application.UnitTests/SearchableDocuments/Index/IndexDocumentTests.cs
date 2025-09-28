using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.SearchableDocuments.Index;
using Lanka.Modules.Matching.Domain.SearchableDocuments;
using NSubstitute;

namespace Lanka.Modules.Matching.Application.UnitTests.SearchableDocuments.Index;

public class IndexDocumentTests
{
    private static IndexDocumentCommand Command => new(
        SearchableDocumentData.SourceEntityId,
        SearchableDocumentData.Type,
        SearchableDocumentData.Title,
        SearchableDocumentData.Content,
        SearchableDocumentData.Tags,
        SearchableDocumentData.Metadata
    );

    private readonly ISearchIndexService _searchIndexServiceMock;
    private readonly IndexDocumentCommandHandler _handler;

    public IndexDocumentTests()
    {
        this._searchIndexServiceMock = Substitute.For<ISearchIndexService>();
        this._handler = new IndexDocumentCommandHandler(this._searchIndexServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDocumentIsValid()
    {
        // Arrange
        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await this._searchIndexServiceMock.Received(1)
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDocumentCreationFails()
    {
        // Arrange
        var invalidCommand = new IndexDocumentCommand(
            SearchableDocumentData.SourceEntityId,
            SearchableDocumentData.Type,
            SearchableDocumentData.EmptyTitle, // Invalid title
            SearchableDocumentData.Content,
            SearchableDocumentData.Tags,
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
    public async Task Handle_ShouldPassCorrectDocumentToIndexService()
    {
        // Arrange
        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchableDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        await this._searchIndexServiceMock.Received(1).IndexDocumentAsync(
            Arg.Is<SearchableDocument>(doc => 
                doc.SourceEntityId == Command.SourceEntityId &&
                doc.Type == Command.Type &&
                doc.Title.Value == Command.Title &&
                doc.Content.Value == Command.Content),
            Arg.Any<CancellationToken>()
        );
    }
}
