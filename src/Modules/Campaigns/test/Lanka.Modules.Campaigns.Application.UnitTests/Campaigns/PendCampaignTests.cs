using System.Reflection;
using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Campaigns.Pend;
using Lanka.Modules.Campaigns.Application.UnitTests.Offers;
using Lanka.Modules.Campaigns.Application.UnitTests.Pacts;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Campaigns;

#pragma warning disable CA1515 // Type can be made internal
public class PendCampaignTests
#pragma warning restore CA1515
{
    private static PendCampaignCommand Command => new(
        CampaignData.Name,
        CampaignData.Description,
        CampaignData.ScheduledOnUtc,
        OfferId.New()
    );

    private readonly IPactRepository _pactRepositoryMock;
    private readonly ICampaignRepository _campaignRepositoryMock;
    private readonly IOfferRepository _offerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserContext _userContextMock;

    private readonly PendCampaignCommandHandler _handler;

    public PendCampaignTests()
    {
        this._pactRepositoryMock = Substitute.For<IPactRepository>();
        this._campaignRepositoryMock = Substitute.For<ICampaignRepository>();
        this._offerRepositoryMock = Substitute.For<IOfferRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        this._userContextMock = Substitute.For<IUserContext>();

        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(CampaignData.UtcNow);

        this._handler = new PendCampaignCommandHandler(
            this._pactRepositoryMock,
            this._campaignRepositoryMock,
            this._offerRepositoryMock,
            this._unitOfWorkMock,
            dateTimeProvider,
            this._userContextMock
        );
    }

    [Fact]
    public async Task Handle_ShouldPendCampaign()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();
        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(offer);

        Pact pact = PactData.CreatePact();
        this._pactRepositoryMock.GetByIdWithOffersAsync(
            Arg.Any<PactId>(),
            Arg.Any<CancellationToken>()
        ).Returns(pact);
        
        // Use reflection to access the private field "_offers"
        FieldInfo? offersField = typeof(Pact).GetField(
            "_offers",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        if (offersField is not null)
        {
            var offers = (List<Offer>)offersField.GetValue(pact);
            offers!.Add(offer);
        }
        
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());

        this._campaignRepositoryMock.IsAlreadyStartedAsync(
            offer,
            Command.ScheduledOnUtc,
            Arg.Any<CancellationToken>()
        ).Returns(false);
        
        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenOfferDoesNotExist()
    {
        // Arrange
        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Offer?)null);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenPactDoesNotExist()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();
        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(offer);

        this._pactRepositoryMock.GetByIdWithOffersAsync(
            Arg.Any<PactId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Pact?)null);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSameUserError_WhenPactIsCreatedBySameUser()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();
        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(offer);

        Pact pact = PactData.CreatePact();
        this._pactRepositoryMock.GetByIdWithOffersAsync(
            Arg.Any<PactId>(),
            Arg.Any<CancellationToken>()
        ).Returns(pact);

        this._userContextMock.GetUserId().Returns(pact.BloggerId.Value);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnAlreadyStartedError_WhenCampaignIsAlreadyStarted()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();
        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(offer);

        Pact pact = PactData.CreatePact();
        this._pactRepositoryMock.GetByIdWithOffersAsync(
            Arg.Any<PactId>(),
            Arg.Any<CancellationToken>()
        ).Returns(pact);

        this._userContextMock.GetUserId().Returns(Guid.NewGuid());

        this._campaignRepositoryMock.IsAlreadyStartedAsync(
            offer,
            Command.ScheduledOnUtc,
            Arg.Any<CancellationToken>()
        ).Returns(true);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPendCampaignFails()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();
        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(offer);

        Pact pact = PactData.CreatePact();
        this._pactRepositoryMock.GetByIdWithOffersAsync(
            Arg.Any<PactId>(),
            Arg.Any<CancellationToken>()
        ).Returns(pact);

        this._userContextMock.GetUserId().Returns(Guid.NewGuid());

        this._campaignRepositoryMock.IsAlreadyStartedAsync(
            offer,
            Command.ScheduledOnUtc,
            Arg.Any<CancellationToken>()
        ).Returns(false);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
