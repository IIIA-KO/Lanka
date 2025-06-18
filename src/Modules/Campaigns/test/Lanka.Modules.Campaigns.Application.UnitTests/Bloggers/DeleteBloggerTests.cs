using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Bloggers.Delete;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Bloggers;

#pragma warning disable CA1515
public class DeleteBloggerTests
{
    private readonly IBloggerRepository _bloggerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly DeleteBloggerCommandHandler _handler;

    public DeleteBloggerTests()
    {
        this._bloggerRepositoryMock = Substitute.For<IBloggerRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new DeleteBloggerCommandHandler(
            this._bloggerRepositoryMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Arrange
        var command = new DeleteBloggerCommand(Guid.NewGuid());
        var blogger = Blogger.Create(
            command.BloggerId,
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.Email,
            BloggerData.BirthDate
        );

        this._bloggerRepositoryMock
            .GetByIdAsync(
                Arg.Any<BloggerId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(blogger);

        // Act
        Result result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        this._bloggerRepositoryMock.Received(1).Remove(blogger);
        await this._unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenBloggerNotFound()
    {
        // Arrange
        var command = new DeleteBloggerCommand(Guid.NewGuid());

        this._bloggerRepositoryMock
            .GetByIdAsync(
                Arg.Any<BloggerId>(),
                Arg.Any<CancellationToken>()
            ).Returns((Blogger?)null);

        // Act
        Result result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BloggerErrors.NotFound);

        await this._unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        this._bloggerRepositoryMock.DidNotReceive().Remove(Arg.Any<Blogger>());
    }
}
