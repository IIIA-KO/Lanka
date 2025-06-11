using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetTableStatistics;

internal sealed class GetTableStatisticsQueryValidator
    : AbstractValidator<GetTableStatisticsQuery>
{
    public GetTableStatisticsQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
