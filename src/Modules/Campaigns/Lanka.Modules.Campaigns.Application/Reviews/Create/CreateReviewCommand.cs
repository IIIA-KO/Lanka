using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Reviews.Create;

public sealed record CreateReviewCommand(
    Guid CampaignId,
    int Rating,
    string Comment
) : ICommand<Guid>;
