using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Reviews.Create;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Reviews;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Reviews;

#pragma warning disable CA1515 // Type can be made internal
public class CreateReviewTests
#pragma warning restore CA1515
{
    private static CreateReviewCommand Command =>
        new(
            Guid.NewGuid(),
            ReviewData.ValidRating,
            ReviewData.ValidComment
        );

    private readonly ICampaignRepository _campaignRepositoryMock;
    private readonly IReviewRepository _reviewRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewTests()
    {
        this._campaignRepositoryMock = Substitute.For<ICampaignRepository>();
        this._reviewRepositoryMock = Substitute.For<IReviewRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(ReviewData.CreatedOnUtc);

        this._handler = new CreateReviewCommandHandler(
            this._campaignRepositoryMock,
            this._reviewRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock,
            dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnReviewId()
    {
        // Arrange
        Campaign campaign = ReviewData.CompletedCooperation;

        this._campaignRepositoryMock.GetByIdAsync(
                Arg.Any<CampaignId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(campaign);

        this._userContextMock.GetUserId().Returns(campaign.ClientId.Value);

        this._reviewRepositoryMock
            .GetByCampaignIdAndClientIdAsync(
                Arg.Is<CampaignId>(x => x == campaign.Id),
                Arg.Is<BloggerId>(x => x == campaign.ClientId),
                Arg.Any<CancellationToken>()
            )
            .Returns((Review?)null);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        this._campaignRepositoryMock.GetByIdAsync(
                Arg.Any<CampaignId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Campaign?)null);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotAuthorizedError_WhenUserIsNotClient()
    {
        // Arrange
        Campaign campaign = ReviewData.CompletedCooperation;

        this._campaignRepositoryMock.GetByIdAsync(
                Arg.Any<CampaignId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(campaign);

        this._userContextMock.GetUserId().Returns(Guid.NewGuid());

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotAuthorized);
    }

    [Fact]
    public async Task Handle_ShouldReturnAlreadyReviewedError_WhenUserHasAlreadyReviewed()
    {
        // Arrange
        Campaign campaign = ReviewData.CompletedCooperation;

        this._campaignRepositoryMock.GetByIdAsync(
                Arg.Any<CampaignId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(campaign);

        this._userContextMock.GetUserId().Returns(campaign.ClientId.Value);

        this._reviewRepositoryMock
            .GetByCampaignIdAndClientIdAsync(
                Arg.Any<CampaignId>(),
                Arg.Any<BloggerId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ReviewData.CreateReview());

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReviewErrors.AlreadyReviewed);
    }
}
