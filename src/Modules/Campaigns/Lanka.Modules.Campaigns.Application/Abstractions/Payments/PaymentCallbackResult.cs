namespace Lanka.Modules.Campaigns.Application.Abstractions.Payments;

public sealed record PaymentCallbackResult(
    string OrderId,
    bool IsSuccessful);
