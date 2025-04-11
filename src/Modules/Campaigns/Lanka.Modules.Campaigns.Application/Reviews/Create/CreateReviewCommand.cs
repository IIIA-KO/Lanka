using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Reviews.Create;

public sealed record CreateReviewCommand(
    int Rating,
    Guid CampaignId,
    string Comment
) : ICommand;
