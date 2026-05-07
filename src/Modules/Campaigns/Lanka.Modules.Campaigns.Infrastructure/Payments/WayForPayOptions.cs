namespace Lanka.Modules.Campaigns.Infrastructure.Payments;

internal sealed class WayForPayOptions
{
    public string MerchantAccount { get; init; } = string.Empty;

    public string MerchantSecretKey { get; init; } = string.Empty;

    public string MerchantDomainName { get; init; } = string.Empty;

    public string PaymentUrl { get; init; } = "https://secure.wayforpay.com/pay";

    public string PublicBaseUrl { get; init; } = string.Empty;

    public string ServiceUrl { get; init; } = string.Empty;

    public string ReturnUrl { get; init; } = string.Empty;

    public string ClientReturnUrl { get; init; } = string.Empty;
}
