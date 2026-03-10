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
    private readonly IInstagramServiceFactory<IInstagramAudienceService> _instagramAudienceServiceFactory;

    public GetReachDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramAudienceService> instagramAudienceServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceServiceFactory = instagramAudienceServiceFactory;
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

        IInstagramAudienceService audienceService = this._instagramAudienceServiceFactory
            .GetService(account.Email.Value);

        Result<ReachDistribution> result = await audienceService.GetAudienceReachPercentage(
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
