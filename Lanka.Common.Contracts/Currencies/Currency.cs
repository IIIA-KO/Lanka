namespace Lanka.Modules.Pricing.Contracts.Currencies
{
    public sealed record Currency
    {
        private static readonly IReadOnlyCollection<Currency> _all = [
            new("USD"), new("EUR"), new("UAH"), new("GBP"),
            new("CAD"), new("AUD"), new("JPY"), new("CNY"),
            new("INR"), new("CHF"), new("SEK"), new("NZD"),
            new("ZAR"), new("SDG"), new("HKD"), new("NOK"),
            new("MXN"), new("BRL"), new("KRW"), new("TRY")
        ];
        
        public string Code { get; }

        private Currency(string code) => this.Code = code;

        public static Currency FromCode(string code)
        {
            return _all.FirstOrDefault(currency =>
                currency.Code.Equals(code, StringComparison.OrdinalIgnoreCase)
            ) ?? throw new InvalidCastException("The currency is invalid.");
        }
        
        public override string ToString() => this.Code;
    }
}
