using FluentValidation;

namespace Lanka.Modules.Campaigns.Application.Pacts.Edit;

internal sealed class EditPactCommandValidator : AbstractValidator<EditPactCommand>
{
    public EditPactCommandValidator()
    {
        this.RuleFor(x => x.Content)
            .NotNull()
            .NotEmpty()
            .WithMessage("Pact content is required");
    }
}
