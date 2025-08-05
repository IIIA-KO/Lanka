using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackCampaignCompleted;

internal sealed class TrackCampaignCompletedCommandHandler
    : ICommandHandler<TrackCampaignCompletedCommand>
{
    private readonly IUserActivityRepository _userActivityRepository;

    public TrackCampaignCompletedCommandHandler(IUserActivityRepository userActivityRepository)
    {
        this._userActivityRepository = userActivityRepository;
    }

    public async Task<Result> Handle(TrackCampaignCompletedCommand request, CancellationToken cancellationToken)
    {
        UserActivity clientActivity =
            await this._userActivityRepository.GetAsync(
                request.ClientId,
                cancellationToken
            )
            ?? UserActivity.Create(request.ClientId);

        clientActivity.CampaignsCompletedAsClient.Add(request.CompletedAtUtc);
        
        UserActivityCalculator.Calculate(clientActivity, request.CompletedAtUtc);

        await this._userActivityRepository.ReplaceAsync(clientActivity, cancellationToken);

        UserActivity creatorActivity =
            await this._userActivityRepository.GetAsync(
                request.CreatorId,
                cancellationToken
            )
            ?? UserActivity.Create(request.CreatorId);
        
        creatorActivity.CampaignsCompletedCreator.Add(request.CompletedAtUtc);
        
        UserActivityCalculator.Calculate(creatorActivity, request.CompletedAtUtc);
        
        await this._userActivityRepository.ReplaceAsync(creatorActivity, cancellationToken);
        
        return Result.Success();
    }
}
