using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;

internal sealed class GetOverviewStatisticsQueryValidator
    : AbstractValidator<GetOverviewStatisticsQuery>
{
    public GetOverviewStatisticsQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
