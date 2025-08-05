using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;

internal sealed class GetReachDistributionQueryValidator
    : AbstractValidator<GetReachDistributionQuery>
{
    public GetReachDistributionQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
