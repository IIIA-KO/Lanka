using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetMetricsStatistics;

internal sealed class GetMetricsStatisticsQueryHandler
    : IQueryHandler<GetMetricsStatisticsQuery, MetricsStatisticsResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramStatisticsService _instagramStatisticsService;

    public GetMetricsStatisticsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramStatisticsService instagramStatisticsService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramStatisticsService = instagramStatisticsService;
    }

    public async Task<Result<MetricsStatisticsResponse>> Handle(
        GetMetricsStatisticsQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<MetricsStatisticsResponse>(InstagramAccountErrors.NotFound);
        }


        Result<MetricsStatistics> result = await this._instagramStatisticsService.GetMetricsStatistics(
            account,
            request.StatisticsPeriod,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<MetricsStatisticsResponse>(result.Error);
        }

        return MetricsStatisticsResponse.FromMetricStatistics(result.Value);
    }
}
