using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Offers.Edit;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using Lanka.Modules.Campaigns.Application.UnitTests.Pacts;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Offers;

#pragma warning disable CA1515 // Type can be made internal
public class EditOfferTests
#pragma warning restore CA1515
{
    private static EditOfferCommand Command => 
        new (
            Guid.NewGuid(),
            OfferData.Name,
            OfferData.Price.Amount,
            OfferData.Price.Currency.ToString(),
            OfferData.Description
        );
    
    private readonly IOfferRepository _offerRepositoryMock;
    private readonly IPactRepository _pactRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly EditOfferCommandHandler _handler;
    
    public EditOfferTests()
    {
        this._offerRepositoryMock = Substitute.For<IOfferRepository>();
        this._pactRepositoryMock = Substitute.For<IPactRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new EditOfferCommandHandler(
            this._offerRepositoryMock,
            this._pactRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnOfferResponse()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());

        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(OfferData.CreateOffer());

        this._offerRepositoryMock.HasActiveCampaignsAsync(
            Arg.Any<Offer>(),
            Arg.Any<CancellationToken>()
        ).Returns(false);
        
        // Act
        Result<OfferResponse> result = await this._handler.Handle(Command, default);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPactNotFoundError_WhenPactIsNotFound()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Pact?) null);
        
        // Act
        Result<OfferResponse> result = await this._handler.Handle(Command, default);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PactErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnOfferNotFoundError_WhenOfferIsNotFound()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());

        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Offer?) null);
        
        // Act
        Result<OfferResponse> result = await this._handler.Handle(Command, default);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OfferErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnOfferHasActiveCampaignsError_WhenOfferHasActiveCampaigns()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());

        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(OfferData.CreateOffer());

        this._offerRepositoryMock.HasActiveCampaignsAsync(
            Arg.Any<Offer>(),
            Arg.Any<CancellationToken>()
        ).Returns(true);
        
        // Act
        Result<OfferResponse> result = await this._handler.Handle(Command, default);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OfferErrors.HasActiveCampaigns);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOfferIsInvalid()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());

        this._offerRepositoryMock.GetByIdAsync(
            Arg.Any<OfferId>(),
            Arg.Any<CancellationToken>()
        ).Returns(OfferData.CreateOffer());

        this._offerRepositoryMock.HasActiveCampaignsAsync(
            Arg.Any<Offer>(),
            Arg.Any<CancellationToken>()
        ).Returns(false);
        
        var command = new EditOfferCommand(
            Guid.NewGuid(),
            string.Empty,
            OfferData.Price.Amount,
            OfferData.Price.Currency.ToString(),
            OfferData.Description
        );
        
        // Act
        Result<OfferResponse> result = await this._handler.Handle(command, default);
        
        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
