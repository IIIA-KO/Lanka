using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Application.Bloggers.Photos.SetProfilePhoto;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Bloggers;

#pragma warning disable CA1515
public class SetProfilePhotoTests
{
    private static SetProfilePhotoCommand Command => new(Substitute.For<IFormFile>());

    private readonly IPhotoAccessor _photoAccessorMock;
    private readonly IBloggerRepository _bloggerRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly SetProfilePhotoCommandHandler _handler;

    public SetProfilePhotoTests()
    {
        this._photoAccessorMock = Substitute.For<IPhotoAccessor>();
        this._bloggerRepositoryMock = Substitute.For<IBloggerRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new SetProfilePhotoCommandHandler(
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

        this._photoAccessorMock
            .AddPhotoAsync(Arg.Any<IFormFile>())
            .Returns(BloggerData.NewProfilePhoto);

        this._bloggerRepositoryMock
            .GetByIdAsync(new BloggerId(userId), Arg.Any<CancellationToken>())
            .Returns(blogger);

        this._photoAccessorMock.DeletePhotoAsync(BloggerData.OldProfilePhoto.Id)
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await this._photoAccessorMock.Received(1).DeletePhotoAsync(BloggerData.OldProfilePhoto.Id);
        await this._unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAddPhotoFails()
    {
        // Arrange
        this._photoAccessorMock
            .AddPhotoAsync(Arg.Any<IFormFile>())
            .Returns(Result.Failure<Photo>(PhotoErrors.FailedUpload));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhotoErrors.FailedUpload);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_IfOldPhotoDeletionFails()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this._userContextMock.GetUserId().Returns(userId);

        Blogger blogger = BloggerData.CreateBlogger();
        blogger.SetProfilePhoto(BloggerData.OldProfilePhoto);

        this._photoAccessorMock
            .AddPhotoAsync(Arg.Any<IFormFile>())
            .Returns(BloggerData.NewProfilePhoto);
        
        this._bloggerRepositoryMock
            .GetByIdAsync(
                new BloggerId(userId),
                Arg.Any<CancellationToken>()
            ).Returns(blogger);

        this._photoAccessorMock
            .AddPhotoAsync(Command.File)
            .Returns(BloggerData.NewProfilePhoto);

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

    [Fact]
    public async Task Handle_ShouldSetPhotoAndSave_WhenEverythingSucceeds_AndNoOldPhoto()
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

        this._photoAccessorMock
            .AddPhotoAsync(Arg.Any<IFormFile>())
            .Returns(BloggerData.NewProfilePhoto);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        blogger.ProfilePhoto.Should().BeEquivalentTo(BloggerData.NewProfilePhoto);

        await this._photoAccessorMock
            .DidNotReceive()
            .DeletePhotoAsync(Arg.Any<string>());

        await this._unitOfWorkMock
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
