using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetTableStatistics;

public sealed record GetTableStatisticsQuery(Guid UserId, StatisticsPeriod StatisticsPeriod)
    : ICachedQuery<TableStatistics>
{
    public string CacheKey => $"table-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
