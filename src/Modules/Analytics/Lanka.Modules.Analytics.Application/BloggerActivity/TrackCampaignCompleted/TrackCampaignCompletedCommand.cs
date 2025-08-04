using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackCampaignCompleted;

public record TrackCampaignCompletedCommand(
    Guid ClientId, 
    Guid CreatorId,
    DateTimeOffset CompletedAtUtc
) : ICommand;
