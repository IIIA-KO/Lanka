namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;

public sealed record CampaignResponse(
    Guid Id,
    string Status,
    string Name,
    string Description,
    Guid OfferId,
    Guid ClientId,
    Guid CreatorId,
    DateTime ScheduledOnUtc,
    DateTime PendedOnUtc,
    DateTime? ConfirmedOnUtc,
    DateTime? RejectedOnUtc,
    DateTime? CancelledOnUtc,
    DateTime? DoneOnUtc,
    DateTime? CompletedOnUtc,
    decimal PriceAmount,
    string PriceCurrency,
    string CreatorFirstName,
    string CreatorLastName,
    string ClientFirstName,
    string ClientLastName,
    bool HasReview
);
