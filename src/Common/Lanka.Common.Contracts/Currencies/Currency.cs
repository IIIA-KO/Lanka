namespace Lanka.Common.Contracts.Currencies;

public sealed record Currency
{
    private static readonly IReadOnlyCollection<Currency> _all =
        Enum.GetValues<CurrencyCode>().Select(code => new Currency(code)).ToList();

    public CurrencyCode Code { get; }

    private Currency(CurrencyCode code)
    {
        this.Code = code;
    }

    public static Currency FromCode(string code)
    {
        return _all.FirstOrDefault(currency =>
            currency.Code.ToString().Equals(code, StringComparison.OrdinalIgnoreCase)
        ) ?? throw new InvalidCastException("The currency is invalid.");
    }

    public override string ToString()
    {
        return this.Code.ToString();
    }
}
