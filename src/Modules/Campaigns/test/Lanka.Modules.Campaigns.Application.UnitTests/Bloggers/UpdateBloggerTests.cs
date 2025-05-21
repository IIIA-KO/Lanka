using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Campaigns.Application.Bloggers.Update;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Bloggers;

#pragma warning disable CA1515 // Type can be made internal
public class UpdateBloggerTests
#pragma warning restore CA1515
{
    private static UpdateBloggerCommand Command =>
        new(
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.BirthDate,
            BloggerData.Bio
        );

    private readonly IBloggerRepository _bloggerRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly UpdateBloggerCommandHandler _handler;

    public UpdateBloggerTests()
    {
        this._bloggerRepositoryMock = Substitute.For<IBloggerRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new UpdateBloggerCommandHandler(
            this._bloggerRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task UpdateBlogger_ShouldReturnSuccess()
    {
        // Arrange
        Blogger blogger = BloggerData.CreateBlogger();
        this._bloggerRepositoryMock.GetByIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(blogger);

        this._userContextMock.GetUserId().Returns(blogger.Id.Value);

        // Act
        Result<BloggerResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateBlogger_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        this._bloggerRepositoryMock.GetByIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Blogger?)null);

        // Act
        Result<BloggerResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BloggerErrors.NotFound);
    }

    [Fact]
    public async Task UpdateBlogger_ShouldReturnFailure_WhenUserNotAuthorized()
    {
        // Arrange
        Blogger blogger = BloggerData.CreateBlogger();
        this._bloggerRepositoryMock.GetByIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(blogger);

        this._userContextMock.GetUserId().Returns(Guid.NewGuid());

        // Act
        Result<BloggerResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotAuthorized);
    }

    [Fact]
    public async Task UpdateBlogger_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        var bloggerId = Guid.NewGuid();
        this._userContextMock.GetUserId().Returns(bloggerId);
        
        this._bloggerRepositoryMock.GetByIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(BloggerData.CreateBlogger());


        var command = new UpdateBloggerCommand(
            string.Empty,
            string.Empty,
            BloggerData.BirthDate,
            BloggerData.Bio
        );

        // Act
        Result<BloggerResponse> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
