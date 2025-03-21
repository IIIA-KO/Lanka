using Lanka.Common.Domain;

namespace Lanka.Common.Contracts.Currencies;

public static class CurrencyErrors
{
    public static readonly Error Empty =
        Error.Validation("Currency.Empty", "Currency code cannot be empty.");

    public static readonly Error Invalid =
        Error.Validation("Currency.Invalid", "The currency code is not supported.");

    public static readonly Error InvalidFormat =
        Error.Validation("Currency.InvalidFormat", "Currency code must be a 3-letter ISO code.");
}
