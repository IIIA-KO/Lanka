using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Offers.Delete;
using Lanka.Modules.Campaigns.Application.UnitTests.Pacts;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Offers;

#pragma warning disable CA1515 // Type can be made internal
public class DeleteOfferTests
#pragma warning restore CA1515
{
    private static DeleteOfferCommand Command => new(Guid.NewGuid());
    
    private readonly IOfferRepository _offerRepositoryMock;
    private readonly IPactRepository _pactRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly DeleteOfferCommandHandler _handler;

    public DeleteOfferTests()
    {
        this._offerRepositoryMock = Substitute.For<IOfferRepository>();
        this._pactRepositoryMock = Substitute.For<IPactRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        this._handler = new DeleteOfferCommandHandler(
            this._offerRepositoryMock,
            this._pactRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOfferDeleted()
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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

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
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OfferErrors.HasActiveCampaigns);
    }
}
