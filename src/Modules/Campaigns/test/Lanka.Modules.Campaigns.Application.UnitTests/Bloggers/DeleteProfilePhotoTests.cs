using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Application.Bloggers.Photos.DeleteProfilePhoto;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Bloggers;

#pragma warning disable CA1515
public class DeleteProfilePhotoTests
{
    private static DeleteProfilePhotoCommand Command => new();

    private readonly IPhotoAccessor _photoAccessorMock;
    private readonly IBloggerRepository _bloggerRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly DeleteProfilePhotoCommandHandler _handler;

    public DeleteProfilePhotoTests()
    {
        this._photoAccessorMock = Substitute.For<IPhotoAccessor>();
        this._bloggerRepositoryMock = Substitute.For<IBloggerRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new DeleteProfilePhotoCommandHandler(
            this._photoAccessorMock,
            this._bloggerRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this._userContextMock.GetUserId().Returns(userId);

        Blogger blogger = BloggerData.CreateBlogger();
        blogger.SetProfilePhoto(BloggerData.OldProfilePhoto);

        this._bloggerRepositoryMock
            .GetByIdAsync(
                new BloggerId(userId),
                Arg.Any<CancellationToken>()
            ).Returns(blogger);

        this._photoAccessorMock
            .DeletePhotoAsync(BloggerData.OldProfilePhoto.Id)
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        blogger.ProfilePhoto.Should().BeNull();

        await this._photoAccessorMock
            .Received(1)
            .DeletePhotoAsync(BloggerData.OldProfilePhoto.Id);

        await this._unitOfWorkMock
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPhotoIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this._userContextMock.GetUserId().Returns(userId);

        Blogger blogger = BloggerData.CreateBlogger();

        this._bloggerRepositoryMock
            .GetByIdAsync(
                new BloggerId(userId),
                Arg.Any<CancellationToken>()
            ).Returns(blogger);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhotoErrors.PhotoNotFound);

        await this._photoAccessorMock
            .DidNotReceive()
            .DeletePhotoAsync(Arg.Any<string>());

        await this._unitOfWorkMock
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeletePhotoFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this._userContextMock.GetUserId().Returns(userId);

        Blogger blogger = BloggerData.CreateBlogger();
        blogger.SetProfilePhoto(BloggerData.OldProfilePhoto);

        this._bloggerRepositoryMock
            .GetByIdAsync(
                new BloggerId(userId),
                Arg.Any<CancellationToken>()
            ).Returns(blogger);

        this._photoAccessorMock
            .DeletePhotoAsync(BloggerData.OldProfilePhoto.Id)
            .Returns(Result.Failure(PhotoErrors.FailedDelete));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhotoErrors.FailedDelete);

        await this._unitOfWorkMock
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
