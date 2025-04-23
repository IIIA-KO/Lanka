using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Campaigns.Reject;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Campaigns;

#pragma warning disable CA1515 // Type can be made internal
public class RejectCampaignTests
#pragma warning restore CA1515
{
    private static RejectCampaignCommand Campaign => new(Guid.NewGuid());
    
    private readonly ICampaignRepository _campaignRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly RejectCampaignCommandHandler _handler;

    public RejectCampaignTests()
    {
        this._campaignRepositoryMock = Substitute.For<ICampaignRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(CampaignData.UtcNow);
        
        this._handler = new RejectCampaignCommandHandler(
            this._campaignRepositoryMock,
            this._userContextMock,
            dateTimeProvider,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldRejectCampaign()
    {
        // Arrange
        Campaign campaign = CampaignData.CreatePendingCampaign();
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        this._userContextMock.GetUserId().Returns(campaign.CreatorId.Value);
        
        // Act
        Result result = await this._handler.Handle(Campaign, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Rejected);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns((Campaign?)null);
        
        // Act
        Result result = await this._handler.Handle(Campaign, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotAuthorizedError_WhenUserIsNotCreator()
    {
        // Arrange
        Campaign campaign = CampaignData.CreatePendingCampaign();
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        
        // Act
        Result result = await this._handler.Handle(Campaign, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotAuthorized);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotPendingError_WhenCampaignIsAlreadyRejected()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateConfirmedCampaign();
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        this._userContextMock.GetUserId().Returns(campaign.CreatorId.Value);
        
        // Act
        Result result = await this._handler.Handle(Campaign, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotPending);
    }
}
