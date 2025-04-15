using System.Collections.Specialized;
using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;
using Lanka.Modules.Campaigns.UnitTests.Offers;

namespace Lanka.Modules.Campaigns.UnitTests.Campaigns;

public class CampaignTests : BaseTest
{
    #region Pend
    [Fact]
    public void Pend_ShouldReturnCampaign()
    {
        // Act
        Result<Campaign> result = Campaign
            .Pend(
                CampaignData.Name,
                CampaignData.Description,
                DateTimeOffset.UtcNow.AddDays(7),
                OfferData.CreateOffer(),
                BloggerId.New(),
                BloggerId.New(),
                DateTimeOffset.UtcNow
            );

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Pend_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Act
        Result<Campaign> result = Campaign
            .Pend(
                string.Empty,
                CampaignData.Description,
                DateTimeOffset.UtcNow.AddDays(7),
                OfferData.CreateOffer(),
                BloggerId.New(),
                BloggerId.New(),
                DateTimeOffset.UtcNow
            );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Pend_ShouldReturnFailure_WhenDescriptionIsEmpty()
    {
        // Act
        Result<Campaign> result = Campaign
            .Pend(
                CampaignData.Name,
                string.Empty,
                DateTimeOffset.UtcNow.AddDays(7),
                OfferData.CreateOffer(),
                BloggerId.New(),
                BloggerId.New(),
                DateTimeOffset.UtcNow
            );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Pend_ShouldReturnFailure_WhenEndDateIsInThePast()
    {
        // Act
        Result<Campaign> result = Campaign
            .Pend(
                CampaignData.Name,
                CampaignData.Description,
                DateTimeOffset.UtcNow.AddDays(-7),
                OfferData.CreateOffer(),
                BloggerId.New(),
                BloggerId.New(),
                DateTimeOffset.UtcNow
            );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Pend_ShouldReturnFailure_WhenClientAndCreatorAreSame()
    {
        // Arrange
        var bloggerId = BloggerId.New();

        // Act
        Result<Campaign> result = Campaign
            .Pend(
                CampaignData.Name,
                CampaignData.Description,
                DateTimeOffset.UtcNow.AddDays(7),
                OfferData.CreateOffer(),
                bloggerId,
                bloggerId,
                DateTimeOffset.UtcNow
            );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Pend_ShouldRaiseCampaignPendedDomainEvent()
    {
        // Act
        Campaign campaign = CampaignData.CreatePendingCampaign();

        // Assert
        CampaignPendedDomainEvent domainEvent
            = AssertDomainEventWasPublished<CampaignPendedDomainEvent>(campaign);

        domainEvent.CampaignId.Should().Be(campaign.Id);
    }
    #endregion
    
    #region Confirm
    [Fact]
    public void Confirm_ShouldReturnSuccess()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreatePendingCampaign();

        // Act
        Result result = campaign.Confirm(utcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Confirmed);
        campaign.ConfirmedOnUtc.Should().Be(utcNow);
    }
    
    [Fact]
    public void Confirm_ShouldReturnFailure_WhenCampaignIsNotPending()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateConfirmedCampaign();

        // Act
        Result result = campaign.Confirm(DateTimeOffset.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void Confirm_ShouldRaiseCampaignConfirmedDomainEvent()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreatePendingCampaign();

        // Act
        campaign.Confirm(utcNow);

        // Assert
        CampaignConfirmedDomainEvent domainEvent
            = AssertDomainEventWasPublished<CampaignConfirmedDomainEvent>(campaign);

        domainEvent.CampaignId.Should().Be(campaign.Id);
    }
    #endregion

    #region Reject
    [Fact]
    public void Reject_ShouldReturnSuccess()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreatePendingCampaign();

        // Act
        Result result = campaign.Reject(utcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Rejected);
        campaign.RejectedOnUtc.Should().Be(utcNow);
    }
    
    [Fact]
    public void Reject_ShouldReturnFailure_WhenCampaignIsNotPending()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateConfirmedCampaign();

        // Act
        Result result = campaign.Reject(DateTimeOffset.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void Reject_ShouldRaiseCampaignRejectedDomainEvent()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreatePendingCampaign();

        // Act
        campaign.Reject(utcNow);

        // Assert
        CampaignRejectedDomainEvent domainEvent
            = AssertDomainEventWasPublished<CampaignRejectedDomainEvent>(campaign);

        domainEvent.CampaignId.Should().Be(campaign.Id);
    }
    #endregion
    
    #region MarkAsDone
    [Fact]
    public void MarkAsDone_ShouldReturnSuccess()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreateConfirmedCampaign();

        // Act
        Result result = campaign.MarkAsDone(utcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Done);
        campaign.DoneOnUtc.Should().Be(utcNow);
    }
    
    [Fact]
    public void MarkAsDone_ShouldReturnFailure_WhenCampaignIsNotConfirmed()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateDoneCampaign();

        // Act
        Result result = campaign.MarkAsDone(DateTimeOffset.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void MarkAsDone_ShouldRaiseCampaignMarkedAsDoneDomainEvent()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreateConfirmedCampaign();

        // Act
        campaign.MarkAsDone(utcNow);

        // Assert
        CampaignMarkedAsDoneDomainEvent domainEvent
            = AssertDomainEventWasPublished<CampaignMarkedAsDoneDomainEvent>(campaign);

        domainEvent.CampaignId.Should().Be(campaign.Id);
    }
    #endregion
    
    #region Complete
    [Fact]
    public void Complete_ShouldReturnSuccess()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreateDoneCampaign();

        // Act
        Result result = campaign.Complete(utcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Completed);
        campaign.CompletedOnUtc.Should().Be(utcNow);
    }
    
    [Fact]
    public void Complete_ShouldReturnFailure_WhenCampaignIsNotDone()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateCompletedCampaign();

        // Act
        Result result = campaign.Complete(DateTimeOffset.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void Complete_ShouldRaiseCampaignCompletedDomainEvent()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreateDoneCampaign();

        // Act
        campaign.Complete(utcNow);

        // Assert
        CampaignCompletedDomainEvent domainEvent
            = AssertDomainEventWasPublished<CampaignCompletedDomainEvent>(campaign);

        domainEvent.CampaignId.Should().Be(campaign.Id);
    }
    #endregion
    
    #region Cancel
    [Fact]
    public void Cancel_ShouldReturnSuccess()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreateConfirmedCampaign();

        // Act
        Result result = campaign.Cancel(utcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        campaign.Status.Should().Be(CampaignStatus.Cancelled);
        campaign.CancelledOnUtc.Should().Be(utcNow);
    }
    
    [Fact]
    public void Cancel_ShouldReturnFailure_WhenCampaignIsNotConfirmed()
    {
        // Arrange
        Campaign campaign = CampaignData.CreateDoneCampaign();

        // Act
        Result result = campaign.Cancel(DateTimeOffset.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void Cancel_ShouldReturnFailure_WhenCampaignIsAlreadyStarted()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreatePendingCampaign();

        // Act
        Result result = campaign.Cancel(utcNow.AddDays(-1));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void Cancel_ShouldRaiseCampaignCancelledDomainEvent()
    {
        // Arrange
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        Campaign campaign = CampaignData.CreateConfirmedCampaign();

        // Act
        campaign.Cancel(utcNow);

        // Assert
        CampaignCancelledDomainEvent domainEvent
            = AssertDomainEventWasPublished<CampaignCancelledDomainEvent>(campaign);

        domainEvent.CampaignId.Should().Be(campaign.Id);
    }
    #endregion
}
