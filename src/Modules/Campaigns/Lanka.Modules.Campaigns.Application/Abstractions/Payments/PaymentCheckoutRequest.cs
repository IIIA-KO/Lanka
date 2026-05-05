namespace Lanka.Modules.Campaigns.Application.Abstractions.Payments;

public sealed record PaymentCheckoutRequest(
    decimal Amount,
    string Currency,
    string OrderId,
    string Description);
