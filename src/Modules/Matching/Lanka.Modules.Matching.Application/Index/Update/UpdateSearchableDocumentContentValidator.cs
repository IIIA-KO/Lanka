using FluentValidation;

namespace Lanka.Modules.Matching.Application.Index.Update;

internal sealed class UpdateSearchableDocumentContentValidator
    : AbstractValidator<UpdateSearchableDocumentContentCommand>
{
    private const int TitleMaxLength = 200;
    private const int ContentMaxLength = 5000;

    public UpdateSearchableDocumentContentValidator()
    {
        this.RuleFor(x => x.SourceEntityId)
            .NotEmpty()
            .WithMessage("Source entity id is required.");

        this.RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid SearchableItemType.");

        this.RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Title) || !string.IsNullOrWhiteSpace(x.Content))
            .WithMessage("At least Title or Content must be provided for search indexing.");

        this.RuleFor(x => x.Title)
            .MaximumLength(TitleMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage($"Title cannot exceed {TitleMaxLength} characters.");

        this.RuleFor(x => x.Content)
            .MaximumLength(ContentMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage($"Content cannot exceed {ContentMaxLength} characters.");

        this.RuleFor(x => x.Tags)
            .NotNull()
            .WithMessage("Tags cannot be null.");

        this.RuleForEach(x => x.Tags)
            .NotEmpty()
            .WithMessage("Tag cannot be empty.")
            .MaximumLength(50)
            .WithMessage("Tag cannot exceed 50 characters.");
    }
}
