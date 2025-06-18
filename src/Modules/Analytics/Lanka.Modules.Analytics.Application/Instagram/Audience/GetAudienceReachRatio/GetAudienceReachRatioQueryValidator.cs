using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceReachRatio;

internal sealed class GetAudienceReachRatioQueryValidator
    : AbstractValidator<GetAudienceReachRatioQuery>
{
    public GetAudienceReachRatioQueryValidator()
    {
        this.RuleFor(c => c.StatisticsPeriod).IsInEnum();
    }
}
