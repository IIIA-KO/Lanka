using FluentValidation;

namespace Lanka.Modules.Matching.Application.Index.Activate;

internal sealed class ActivateSearchableDocumentCommandValidator : AbstractValidator<ActivateSearchableDocumentCommand>
{
    public ActivateSearchableDocumentCommandValidator()
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
