using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Application.Index.Create;
using NSubstitute;

namespace Lanka.Modules.Matching.Application.UnitTests.SearchableDocuments.Create;

public class CreateDocumentTests
{
    private static CreateDocumentCommand Command => new(
        SearchableDocumentData.SourceEntityId,
        SearchableDocumentData.Type,
        SearchableDocumentData.Title,
        SearchableDocumentData.Content,
        SearchableDocumentData.Tags,
        SearchableDocumentData.Metadata
    );

    private readonly ISearchIndexService _searchIndexServiceMock;
    private readonly CreateDocumentCommandHandler _handler;

    public CreateDocumentTests()
    {
        this._searchIndexServiceMock = Substitute.For<ISearchIndexService>();
        this._handler = new CreateDocumentCommandHandler(this._searchIndexServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldIndexDocumentWithCorrectData_WhenCommandIsExecuted()
    {
        // Arrange
        SearchDocument? capturedDocument = null;

        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Do<SearchDocument>(d => capturedDocument = d), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.Title.Should().Be(Command.Title);
        capturedDocument.Content.Should().Be(Command.Content);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDocumentIsValid()
    {
        // Arrange
        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await this._searchIndexServiceMock.Received(1)
            .IndexDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDocumentCreationFails()
    {
        // Arrange - both title and content empty should fail validation
        var invalidCommand = new CreateDocumentCommand(
            SearchableDocumentData.SourceEntityId,
            SearchableDocumentData.Type,
            SearchableDocumentData.EmptyTitle,
            SearchableDocumentData.EmptyContent,  // Both empty - invalid
            SearchableDocumentData.Tags,
            SearchableDocumentData.Metadata
        );

        // Act
        Result result = await this._handler.Handle(invalidCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await this._searchIndexServiceMock.DidNotReceive()
            .IndexDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIndexingFails()
    {
        // Arrange
        var indexError = Error.Failure("IndexError", "Failed to index document");
        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(indexError));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(indexError);
    }

    [Fact]
    public async Task Handle_ShouldCallIndexService_With_CorrectParameters()
    {
        // Arrange
        this._searchIndexServiceMock
            .IndexDocumentAsync(Arg.Any<SearchDocument>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        await this._searchIndexServiceMock.Received(1).IndexDocumentAsync(
            Arg.Is<SearchDocument>(doc => 
                doc.SourceEntityId == Command.SourceEntityId &&
                doc.Type == Command.Type &&
                doc.Title == Command.Title &&
                doc.Content == Command.Content),
            Arg.Any<CancellationToken>()
        );
    }
}
