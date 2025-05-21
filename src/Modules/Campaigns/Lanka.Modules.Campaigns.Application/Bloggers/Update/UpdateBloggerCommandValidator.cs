using FluentValidation;
using Lanka.Modules.Campaigns.Domain.Bloggers.Bios;
using Lanka.Modules.Campaigns.Domain.Bloggers.FirstNames;
using Lanka.Modules.Campaigns.Domain.Bloggers.LastNames;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Update;

internal sealed class UpdateBloggerCommandValidator : AbstractValidator<UpdateBloggerCommand>
{
    public UpdateBloggerCommandValidator()
    {
        this.RuleFor(c => c.FirstName)
            .NotEmpty()
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(FirstName.MaxLength);

        this.RuleFor(c => c.LastName)
            .NotEmpty()
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(LastName.MaxLength);

        this.RuleFor(c => c.BirthDate)
            .NotEmpty()
            .WithMessage("Birth Date is required.");
        
        this.RuleFor(c => c.Bio)
            .NotNull()
            .MaximumLength(Bio.MaxLength)
            .WithMessage(BioErrors.TooLong(Bio.MaxLength).Description);
    }
}
