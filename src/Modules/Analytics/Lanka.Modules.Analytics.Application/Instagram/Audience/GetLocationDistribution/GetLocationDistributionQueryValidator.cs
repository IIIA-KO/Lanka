using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;

internal sealed class GetLocationDistributionQueryValidator
    : AbstractValidator<GetLocationDistributionQuery>
{
    public GetLocationDistributionQueryValidator()
    {
        this.RuleFor(c => c.LocationType).IsInEnum();
    }
}
