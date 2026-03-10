using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;

internal sealed class GetOverViewStatisticsQueryHandler
    : IQueryHandler<GetOverviewStatisticsQuery, OverviewStatisticsResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramServiceFactory<IInstagramStatisticsService> _instagramStatisticsServiceFactory;

    public GetOverViewStatisticsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramStatisticsService> instagramStatisticsServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramStatisticsServiceFactory = instagramStatisticsServiceFactory;
    }

    public async Task<Result<OverviewStatisticsResponse>> Handle(
        GetOverviewStatisticsQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<OverviewStatisticsResponse>(InstagramAccountErrors.NotFound);
        }

        IInstagramStatisticsService statisticsService = this._instagramStatisticsServiceFactory
            .GetService(account.Email.Value);

        Result<OverviewStatistics> result = await statisticsService.GetOverviewStatistics(
            account,
            request.StatisticsPeriod,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<OverviewStatisticsResponse>(result.Error);
        }

        return OverviewStatisticsResponse.FromOverviewStatistics(result.Value);
    }
}
