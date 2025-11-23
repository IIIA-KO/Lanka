using FluentValidation;

namespace Lanka.Modules.Matching.Application.Index.Deactivate;

internal sealed class DeactivateSearchableDocumentCommandValidator : AbstractValidator<DeactivateSearchableDocumentCommand>
{
    public DeactivateSearchableDocumentCommandValidator()
    {
        this
            .RuleFor(x => x.SourceEntityId)
            .NotEmpty()
            .WithMessage("Source entity id is required.");

        this
            .RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid SearchableItemType.");
    }
}
