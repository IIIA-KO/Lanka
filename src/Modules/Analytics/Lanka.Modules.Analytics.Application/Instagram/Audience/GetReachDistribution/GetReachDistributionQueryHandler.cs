using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;

internal sealed class GetReachDistributionQueryHandler
    : IQueryHandler<GetReachDistributionQuery, ReachDistribution>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramAudienceService _instagramAudienceService;

    public GetReachDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramAudienceService instagramAudienceService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceService = instagramAudienceService;
    }

    public async Task<Result<ReachDistribution>> Handle(
        GetReachDistributionQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<ReachDistribution>(InstagramAccountErrors.NotFound);
        }

        return await this._instagramAudienceService.GetAudienceReachPercentage(
            new InstagramPeriodRequest(
                account.Token!.AccessToken.Value,
                account.Metadata.Id,
                request.StatisticsPeriod
            ),
            cancellationToken
        );
    }
}
