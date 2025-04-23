using FluentAssertions;
using Lanka.Common.Application.Clock;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Campaigns.Cancel;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Campaigns;

#pragma warning disable CA1515 // Type can be made internal
public class CancelCampaignTests
#pragma warning restore CA1515
{
    private static CancelCampaignCommand Command => new(Guid.NewGuid());
    
    private readonly ICampaignRepository _campaignRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly CancelCampaignCommandHandler _handler;
    
    public CancelCampaignTests()
    {
        this._campaignRepositoryMock = Substitute.For<ICampaignRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(CampaignData.UtcNow);
        
        this._handler = new CancelCampaignCommandHandler(
            this._campaignRepositoryMock,
            dateTimeProvider,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldCancelCampaign()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateConfirmedCampaign();
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Cancelled);
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
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotConfirmedError_WhenCampaignIsAlreadyCancelled()
    {
        // Arrange
        Campaign campaign = CampaignData.CreatePendingCampaign();
        campaign.Cancel(CampaignData.UtcNow);
        
        this._campaignRepositoryMock
            .GetByIdAsync(
                Arg.Any<CampaignId>(), 
                Arg.Any<CancellationToken>()
            ).Returns(campaign);
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CampaignErrors.NotConfirmed);
    }
}
