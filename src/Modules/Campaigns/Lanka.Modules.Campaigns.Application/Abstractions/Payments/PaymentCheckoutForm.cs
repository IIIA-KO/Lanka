namespace Lanka.Modules.Campaigns.Application.Abstractions.Payments;

public sealed record PaymentCheckoutForm(
    Uri ActionUrl,
    string Method,
    IReadOnlyDictionary<string, string> Fields);
