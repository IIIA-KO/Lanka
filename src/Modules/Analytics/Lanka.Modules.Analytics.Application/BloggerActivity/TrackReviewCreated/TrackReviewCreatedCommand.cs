using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackReviewCreated;

public sealed record TrackReviewCreatedCommand(
    Guid ClientId,
    Guid CampaignId,
    int Rating,
    DateTimeOffset CreatedAtUtc
) : ICommand;
