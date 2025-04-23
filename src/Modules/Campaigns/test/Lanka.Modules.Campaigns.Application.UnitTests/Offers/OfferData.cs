using Lanka.Common.Contracts.Currencies;
using Lanka.Common.Contracts.Monies;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Offers;

internal static class OfferData
{
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
    
    public static PactId PactId => PactId.New();

    public static string Name => "Test Offer";

    public static string Description => "Test Description";
   
    public static Money Price => Money.Create(1m, Currency.FromCode("UAH")).Value;
}
