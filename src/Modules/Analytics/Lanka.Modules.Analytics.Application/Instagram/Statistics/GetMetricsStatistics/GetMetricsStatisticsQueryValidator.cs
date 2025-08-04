using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetMetricsStatistics;

internal sealed class GetMetricsStatisticsQueryValidator
    : AbstractValidator<GetMetricsStatisticsQuery>
{
    public GetMetricsStatisticsQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
