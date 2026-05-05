using System.Text.Json.Serialization;

namespace Lanka.Modules.Campaigns.Application.Abstractions.Payments;

public sealed record PaymentCallbackResponse(
    [property: JsonPropertyName("orderReference")]
    string OrderReference,
    [property: JsonPropertyName("status")]
    string Status,
    [property: JsonPropertyName("time")]
    long Time,
    [property: JsonPropertyName("signature")]
    string Signature);
