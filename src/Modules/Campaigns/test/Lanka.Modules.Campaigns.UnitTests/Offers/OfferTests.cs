using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Offers;

public class OfferTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnOffer()
    {
        // Act
        Result<Offer> result = Offer.Create(
            OfferData.PactId,
            OfferData.Name,
            OfferData.Description,
            OfferData.Price.Amount,
            OfferData.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Act
        Result<Offer> result = Offer.Create(
            OfferData.PactId,
            string.Empty,
            OfferData.Description,
            OfferData.Price.Amount,
            OfferData.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionIsEmpty()
    {
        // Act
        Result<Offer> result = Offer.Create(
            OfferData.PactId,
            OfferData.Name,
            string.Empty,
            OfferData.Price.Amount,
            OfferData.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPriceAmountIsInvalid()
    {
        // Act
        Result<Offer> result = Offer.Create(
            OfferData.PactId,
            OfferData.Name,
            OfferData.Description,
            -1m,
            OfferData.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPriceCurrencyIsInvalid()
    {
        // Act
        Action action = () => Offer.Create(
            OfferData.PactId,
            OfferData.Name,
            OfferData.Description,
            OfferData.Price.Amount,
            "InvalidCurrency"
        );

        // Assert
        action.Should().Throw<InvalidCastException>()
            .WithMessage("The currency is invalid.");
    }

    [Fact]
    public void Update_ShouldUpdateOfferDetails()
    {
        // Arrange
        string updatedName = "Updated Name";
        string updatedDescription = "Updated Description";

        Offer offer = Offer.Create(
            OfferData.PactId,
            OfferData.Name,
            OfferData.Description,
            OfferData.Price.Amount,
            OfferData.Price.Currency.Code.ToString()
        ).Value;

        // Act
        Result result = offer.Update(
            updatedName,
            updatedDescription,
            OfferData.Price.Amount,
            OfferData.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        offer.Name.Value.Should().Be(updatedName);
        offer.Description.Value.Should().Be(updatedDescription);
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();

        // Act
        Result result = offer.Update(
            string.Empty,
            offer.Description.Value,
            offer.Price.Amount,
            offer.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenDescriptionIsEmpty()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();

        // Act
        Result result = offer.Update(
            offer.Name.Value,
            string.Empty,
            offer.Price.Amount,
            offer.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenPriceAmountIsInvalid()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();

        // Act
        Result result = offer.Update(
            offer.Name.Value,
            offer.Description.Value,
            -1m,
            offer.Price.Currency.Code.ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenPriceCurrencyIsInvalid()
    {
        // Arrange
        Offer offer = OfferData.CreateOffer();

        // Act
        Action action = () => offer.Update(
            offer.Name.Value,
            offer.Description.Value,
            offer.Price.Amount,
            "InvalidCurrency"
        );

        // Assert
        action.Should().Throw<InvalidCastException>()
            .WithMessage("The currency is invalid.");
    }
    
    [Fact]
    public void SetLastCooperatedOnUtc_ShouldSetLastCooperatedOnDate()
    {
        // Arrange
        DateTime lastCooperatedOn = DateTime.UtcNow;
        Offer offer = OfferData.CreateOffer();

        // Act
        offer.SetLastCooperatedOnUtc(lastCooperatedOn);

        // Assert
        offer.LastCooperatedOnUtc.Should().Be(lastCooperatedOn);
    }
}
