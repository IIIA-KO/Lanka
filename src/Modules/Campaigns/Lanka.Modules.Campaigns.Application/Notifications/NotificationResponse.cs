namespace Lanka.Modules.Campaigns.Application.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid CampaignId,
    string Title,
    string Body,
    bool IsRead,
    DateTimeOffset CreatedAtUtc
);
