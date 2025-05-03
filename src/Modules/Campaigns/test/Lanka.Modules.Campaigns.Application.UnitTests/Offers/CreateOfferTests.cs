using System.Reflection;
using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Offers.Create;
using Lanka.Modules.Campaigns.Application.UnitTests.Pacts;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Offers;

#pragma warning disable CA1515 // Type can be made internal
public class CreateOfferTests
#pragma warning restore CA1515
{
    private static CreateOfferCommand Command =>
        new(
            OfferData.Name,
            OfferData.Price.Amount,
            OfferData.Price.Currency.ToString(),
            OfferData.Description
        );

    private readonly IOfferRepository _offerRepositoryMock;
    private readonly IPactRepository _pactRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly CreateOfferCommandHandler _handler;

    public CreateOfferTests()
    {
        this._offerRepositoryMock = Substitute.For<IOfferRepository>();
        this._pactRepositoryMock = Substitute.For<IPactRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new CreateOfferCommandHandler(
            this._offerRepositoryMock,
            this._pactRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnOfferId()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());
        
        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
                Arg.Any<BloggerId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(PactData.CreatePact());

        // Act
        Result<Guid> result = await this._handler.Handle(Command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
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
            )
            .Returns((Pact?)null);

        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PactErrors.NotFound);
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
            )
            .Returns(PactData.CreatePact());

        var command = new CreateOfferCommand(
            string.Empty,
            OfferData.Price.Amount,
            OfferData.Price.Currency.ToString(),
            string.Empty
        );
        
        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnDuplicateError_WhenOfferWithSameNameAlreadyExists()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());
        
        Pact pact = PactData.CreatePact();
        
        this._pactRepositoryMock.GetByBloggerIdWithOffersAsync(
                Arg.Any<BloggerId>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(pact);

        Offer offer = Offer.Create(
            pact.Id,
            Command.Name,
            Command.Description,
            Command.PriceAmount,
            Command.PriceCurrency
        ).Value;

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
        
        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OfferErrors.Duplicate);
    }
}
