using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Offers.Names;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;
using Lanka.Modules.Campaigns.UnitTests.Offers;

namespace Lanka.Modules.Campaigns.UnitTests.Pacts;

public class PactTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnPact()
    {
        // Act
        Result<Pact> pact = Pact.Create(
            PactData.BloggerId,
            PactData.Content
        );

        // Assert
        pact.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenContentIsEmpty()
    {
        // Act
        Result<Pact> result = Pact.Create(
            PactData.BloggerId,
            string.Empty
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnSuccess()
    {
        // Arrange
        Pact pact = Pact.Create(
            PactData.BloggerId,
            PactData.Content
        ).Value;

        // Act
        Result result = pact.Update(PactData.Content);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenContentIsEmpty()
    {
        // Arrange
        Pact pact = Pact.Create(
            PactData.BloggerId,
            PactData.Content
        ).Value;

        // Act
        Result result = pact.Update(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void HasOffer_ShouldReturnFalse_WhenPacDoesNotHaveOffer()
    {
        // Arrange
        Pact pact = Pact.Create(
            PactData.BloggerId,
            PactData.Content
        ).Value;

        // Act
        bool result = pact.HasOffer(Name.Create(OfferData.Name).Value);

        // Assert
        result.Should().BeFalse();
    }
}
