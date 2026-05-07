namespace Lanka.Modules.Campaigns.Application.Payments.GetPayment;

public sealed record PaymentResponse(
    Guid Id,
    Guid CampaignId,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PaidAtUtc
);
