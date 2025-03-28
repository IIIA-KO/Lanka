using FluentValidation;

namespace Lanka.Modules.Campaigns.Application.Pacts.Create;

internal sealed class CreatePactCommandValidator : AbstractValidator<CreatePactCommand>
{
    public CreatePactCommandValidator()
    {
        this.RuleFor(x => x.Content)
            .NotNull()
            .NotEmpty()
            .WithMessage("Pact content is required");
    }
}
