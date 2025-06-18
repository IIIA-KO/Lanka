using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;

internal sealed class GetEngagementStatisticsQueryValidator
    : AbstractValidator<GetEngagementStatisticsQuery>
{
    public GetEngagementStatisticsQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
