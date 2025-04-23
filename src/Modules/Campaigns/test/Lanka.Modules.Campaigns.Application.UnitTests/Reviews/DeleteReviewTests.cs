using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Reviews.Delete;
using Lanka.Modules.Campaigns.Domain.Reviews;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Reviews;

#pragma warning disable CA1515 // Type can be made internal
public class DeleteReviewTests
#pragma warning restore CA1515
{
    private static DeleteReviewCommand Command => new(Guid.NewGuid());
    
    private readonly IReviewRepository _reviewRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly DeleteReviewCommandHandler _handler;
    
    public DeleteReviewTests()
    {
        this._reviewRepositoryMock = Substitute.For<IReviewRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new DeleteReviewCommandHandler(
            this._reviewRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenReviewIsDeleted()
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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReviewErrors.NotOwner);
    }
}
