using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Reviews.Edit;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;
using Lanka.Modules.Campaigns.Domain.Reviews;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Reviews;

#pragma warning disable CA1515 // Type can be made internal
public class EditReviewTests
#pragma warning restore CA1515
{
    public static EditReviewCommand Command =>
        new(
            Guid.NewGuid(),
            ReviewData.ValidRating,
            ReviewData.ValidComment
        );
    
    private readonly IReviewRepository _reviewRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly EditReviewCommandHandler _handler;
    
    public EditReviewTests()
    {
        this._reviewRepositoryMock = Substitute.For<IReviewRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new EditReviewCommandHandler(
            this._reviewRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldReturnReview_WhenReviewIsEdited()
    {
        // Arrange
        Review review = ReviewData.CreateReview();

        this._reviewRepositoryMock.GetByIdAsync(
                Arg.Any<ReviewId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(review);

        this._userContextMock.GetUserId().Returns(review.ClientId.Value);

        // Act
        Result<ReviewResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenReviewDoesNotExist()
    {
        // Arrange
        this._reviewRepositoryMock.GetByIdAsync(
                Arg.Any<ReviewId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Review?)null);

        // Act
        Result<ReviewResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReviewErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotOwner_WhenUserIsNotOwner()
    {
        // Arrange
        Review review = ReviewData.CreateReview();

        this._reviewRepositoryMock.GetByIdAsync(
                Arg.Any<ReviewId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(review);

        this._userContextMock.GetUserId().Returns(Guid.NewGuid());

        // Act
        Result<ReviewResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReviewErrors.NotOwner);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnInvalidReview_WhenReviewIsInvalid()
    {
        // Arrange
        Review review = ReviewData.CreateReview();

        this._reviewRepositoryMock.GetByIdAsync(
                Arg.Any<ReviewId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(review);

        this._userContextMock.GetUserId().Returns(review.ClientId.Value);

        review.Update(0, string.Empty);

        var command = new EditReviewCommand(Guid.NewGuid(), -1, string.Empty);
        
        // Act
        Result<ReviewResponse> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
