using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Campaigns.MarkAsDone;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Campaigns;

#pragma warning disable CA1515 // Type can be made internal
public class MarkCampaignAsDoneTests
#pragma warning restore CA1515
{
    private static MarkCampaignAsDoneCommand Command => new(Guid.NewGuid());
    
    private readonly ICampaignRepository _campaignRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly MarkCampaignAsDoneCommandHandler _handler;

    public MarkCampaignAsDoneTests()
    {
        this._campaignRepositoryMock = Substitute.For<ICampaignRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(CampaignData.UtcNow);
        
        this._handler = new MarkCampaignAsDoneCommandHandler(
            this._campaignRepositoryMock,
            this._userContextMock,
            dateTimeProvider,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldMarkCampaignAsDone()
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
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Done);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenCampaignDoesNotExist()
    {
        // Arrange
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns((Campaign?)null);
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotAuthorizedError_WhenUserIsNotCreator()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateConfirmedCampaign();
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotAuthorized);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotConfirmedError_WhenCampaignIsAlreadyDone()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateDoneCampaign();
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        this._userContextMock.GetUserId().Returns(campaign.CreatorId.Value);
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotConfirmed);
    }
}
