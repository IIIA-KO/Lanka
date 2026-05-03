namespace Lanka.Modules.Campaigns.Application.Abstractions.Payments;

public interface ILiqPayService
{
    (string Data, string Signature) BuildCheckoutParams(
        decimal amount,
        string currency,
        string orderId,
        string description);

    bool VerifySignature(string data, string signature);
}
