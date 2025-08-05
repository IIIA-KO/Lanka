using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;

internal sealed class GetReachDistributionQueryHandler
    : IQueryHandler<GetReachDistributionQuery, ReachDistributionResponse>
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

    public async Task<Result<ReachDistributionResponse>> Handle(
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
            return Result.Failure<ReachDistributionResponse>(InstagramAccountErrors.NotFound);
        }

        Result<ReachDistribution> result = await this._instagramAudienceService.GetAudienceReachPercentage(
            account,
            request.StatisticsPeriod,
            cancellationToken
        );
        
        if (result.IsFailure)
        {
            return Result.Failure<ReachDistributionResponse>(result.Error);
        }

        return ReachDistributionResponse.FromReachDistribution(result.Value);
    }
}
