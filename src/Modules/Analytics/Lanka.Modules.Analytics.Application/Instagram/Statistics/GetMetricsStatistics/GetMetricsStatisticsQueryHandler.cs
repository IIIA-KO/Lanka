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
    private readonly IInstagramServiceFactory<IInstagramStatisticsService> _instagramStatisticsServiceFactory;

    public GetMetricsStatisticsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramStatisticsService> instagramStatisticsServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramStatisticsServiceFactory = instagramStatisticsServiceFactory;
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

        IInstagramStatisticsService statisticsService = this._instagramStatisticsServiceFactory
            .GetService(account.Email.Value);

        Result<MetricsStatistics> result = await statisticsService.GetMetricsStatistics(
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
