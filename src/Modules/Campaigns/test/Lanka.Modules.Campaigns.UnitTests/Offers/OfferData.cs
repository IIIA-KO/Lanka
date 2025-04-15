using Lanka.Common.Contracts.Currencies;
using Lanka.Common.Contracts.Monies;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Offers;

internal static class OfferData
{
    public static PactId PactId => PactId.New();

    public static string Name => BaseTest.Faker.Commerce.ProductName();

    public static string Description => BaseTest.Faker.Lorem.Sentence();

    public static Money Price => Money.Create(1m, Currency.FromCode("UAH")).Value;

    public static Offer CreateOffer()
    {
        return Offer.Create(
            PactId,
            Name,
            Description,
            Price.Amount,
            Price.Currency.ToString()
        ).Value;
    }
}
