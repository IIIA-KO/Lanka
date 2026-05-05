using Lanka.Modules.Campaigns.Domain.Bloggers.Ibans;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.PayoutAccounts;

public sealed class PayoutAccount
{
    public Iban Iban { get; private set; } = null!;

    public string Currency { get; private set; } = null!;

    private PayoutAccount() { }

    public PayoutAccount(Iban iban, string currency)
    {
        this.Iban = iban;
        this.Currency = currency;
    }
}
