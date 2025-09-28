namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;

public sealed record CampaignResponse(
    Guid Id,
    string Status,
    string Name,
    string Description,
    Guid OfferId,
    Guid ClientId,
    Guid CreatorId,
    DateTimeOffset ScheduledOnUtc,
    DateTimeOffset PendedOnUtc,
    DateTimeOffset? ConfirmedOnUtc,
    DateTimeOffset? RejectedOnUtc,
    DateTimeOffset? CancelledOnUtc,
    DateTimeOffset? DoneOnUtc,
    DateTimeOffset? CompletedOnUtc
);
