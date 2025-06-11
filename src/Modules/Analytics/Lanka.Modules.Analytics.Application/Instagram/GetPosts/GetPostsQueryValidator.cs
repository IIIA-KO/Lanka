using FluentValidation;

namespace Lanka.Modules.Analytics.Application.Instagram.GetPosts;

internal sealed class GetPostsQueryValidator : AbstractValidator<GetPostsQuery>
{
    public GetPostsQueryValidator()
    {
        this.RuleFor(c => c.Limit)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Limit cannot be less than 1");
    }
}
