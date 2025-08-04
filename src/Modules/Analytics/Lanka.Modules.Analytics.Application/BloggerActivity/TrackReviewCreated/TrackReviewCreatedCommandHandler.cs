using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackReviewCreated;

internal sealed class TrackReviewCreatedCommandHandler
    : ICommandHandler<TrackReviewCreatedCommand>
{
    private readonly IUserActivityRepository _userActivityRepository;

    public TrackReviewCreatedCommandHandler(IUserActivityRepository userActivityRepository)
    {
        this._userActivityRepository = userActivityRepository;
    }

    public async Task<Result> Handle(TrackReviewCreatedCommand request, CancellationToken cancellationToken)
    {
        UserActivity userActivity =
            await this._userActivityRepository.GetAsync(
                request.ClientId,
                cancellationToken
            )
            ?? UserActivity.Create(request.ClientId);

        userActivity.ReviewsWritten.Add(new UserActivity.ReviewActivity
        {
            CampaignId = request.CampaignId,
            Rating = request.Rating,
            CreatedAt = request.CreatedAtUtc
        });

        UserActivityCalculator.Calculate(userActivity, request.CreatedAtUtc);
        
        await this._userActivityRepository.ReplaceAsync(userActivity, cancellationToken);

        return Result.Success();
    }
}
