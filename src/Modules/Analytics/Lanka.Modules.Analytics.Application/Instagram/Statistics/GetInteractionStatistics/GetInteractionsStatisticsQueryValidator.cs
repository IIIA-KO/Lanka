using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;

internal sealed class GetInteractionsStatisticsQueryValidator
    : AbstractValidator<GetInteractionStatisticsQuery>
{
    public GetInteractionsStatisticsQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
