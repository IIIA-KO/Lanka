namespace Lanka.Modules.Campaigns.Application.Payments.Initiate;

public sealed record PaymentCheckoutResponse(
    Uri ActionUrl,
    string Method,
    IReadOnlyDictionary<string, string> Fields);
