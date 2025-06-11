using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceLocationRatio;

internal sealed class GetAudienceLocationRatioQueryValidator
    : AbstractValidator<GetAudienceLocationRatioQuery>
{
    public GetAudienceLocationRatioQueryValidator()
    {
        this.RuleFor(c => c.LocationType).IsInEnum();
    }
}
