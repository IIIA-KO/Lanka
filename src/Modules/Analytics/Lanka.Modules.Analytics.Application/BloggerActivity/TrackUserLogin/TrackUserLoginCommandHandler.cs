using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackUserLogin;

internal sealed class TrackUserLoginCommandHandler
    : ICommandHandler<TrackUserLoginCommand>
{
    private readonly IUserActivityRepository _userActivityRepository;

    public TrackUserLoginCommandHandler(IUserActivityRepository userActivityRepository)
    {
        this._userActivityRepository = userActivityRepository;
    }

    public async Task<Result> Handle(TrackUserLoginCommand request, CancellationToken cancellationToken)
    {
        UserActivity userActivity =
            await this._userActivityRepository.GetAsync(
                request.UserId,
                cancellationToken
            )
            ?? UserActivity.Create(request.UserId);

        userActivity.LastLoginAt = request.LastLoggedInAtUtc;
        UserActivityCalculator.Calculate(userActivity, request.LastLoggedInAtUtc);
        
        await this._userActivityRepository.ReplaceAsync(userActivity, cancellationToken);

        return Result.Success();
    }
}
