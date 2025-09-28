using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.SearchableDocuments.Remove;
using Lanka.Modules.Matching.Domain.SearchableItems;
using NSubstitute;

namespace Lanka.Modules.Matching.Application.UnitTests.SearchableDocuments.Remove;

public class RemoveDocumentTests
{
    private static RemoveDocumentCommand Command => new(
        SearchableDocumentData.SourceEntityId,
        SearchableDocumentData.Type
    );

    private readonly ISearchIndexService _searchIndexServiceMock;
    private readonly RemoveDocumentCommandHandler _handler;

    public RemoveDocumentTests()
    {
        this._searchIndexServiceMock = Substitute.For<ISearchIndexService>();
        this._handler = new RemoveDocumentCommandHandler(this._searchIndexServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRemovalIsSuccessful()
    {
        // Arrange
        this._searchIndexServiceMock
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
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
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToService()
    {
        // Arrange
        this._searchIndexServiceMock
            .RemoveDocumentsBySourceEntityAsync(
                Arg.Any<Guid>(), 
                Arg.Any<SearchableItemType>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        await this._searchIndexServiceMock.Received(1)
            .RemoveDocumentsBySourceEntityAsync(
                Command.SourceEntityId, 
                Command.Type, 
                Arg.Any<CancellationToken>());
    }
}
