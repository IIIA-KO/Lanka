using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;

internal sealed class GetEngagementStatisticsQueryHandler
    : IQueryHandler<GetEngagementStatisticsQuery, EngagementStatisticsResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramServiceFactory<IInstagramStatisticsService> _instagramStatisticsServiceFactory;

    public GetEngagementStatisticsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramStatisticsService> instagramStatisticsServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramStatisticsServiceFactory = instagramStatisticsServiceFactory;
    }

    public async Task<Result<EngagementStatisticsResponse>> Handle(
        GetEngagementStatisticsQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<EngagementStatisticsResponse>(InstagramAccountErrors.NotFound);
        }

        IInstagramStatisticsService statisticsService = this._instagramStatisticsServiceFactory
            .GetService(account.Email.Value);

        Result<EngagementStatistics> result = await statisticsService.GetEngagementStatistics(
            account,
            request.StatisticsPeriod,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<EngagementStatisticsResponse>(result.Error);
        }
        
        return EngagementStatisticsResponse.FromEngagementStatistics(result.Value);
    }
}
