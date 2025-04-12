using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.Create;

public sealed record CreateReviewCommand(
    Guid CampaignId,
    int Rating,
    string Comment
) : ICommand<ReviewId>;
