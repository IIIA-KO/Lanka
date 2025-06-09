using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramStatisticsService
{
    Task<Result<TableStatistics>> GetTableStatistics(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<OverviewStatistics>> GetOverviewStatistics(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<InteractionStatistics>> GetInteractionsStatistics(
        string instagramAdAccountId,
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<EngagementStatistics>> GetEngagementStatistics(
        int followersCount,
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    );
}
