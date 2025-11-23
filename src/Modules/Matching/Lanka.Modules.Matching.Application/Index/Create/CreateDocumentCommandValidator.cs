using FluentValidation;

namespace Lanka.Modules.Matching.Application.Index.Create;

internal sealed class CreateDocumentCommandValidator
    : AbstractValidator<CreateDocumentCommand>
{
    private const int TitleMaxLength = 200;
    private const int ContentMaxLength = 5000;

    public CreateDocumentCommandValidator()
    {
        this.RuleFor(x => x.SourceEntityId)
            .NotEmpty()
            .WithMessage("Source Entity ID is required.");

        this.RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid SearchableItemType.");

        this.RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Title) || !string.IsNullOrWhiteSpace(x.Content))
            .WithMessage("At least Title or Content must be provided for search indexing.");

        this.RuleFor(x => x.Title)
            .MaximumLength(TitleMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage($"Title must not exceed {TitleMaxLength} characters.");

        this.RuleFor(x => x.Content)
            .MaximumLength(ContentMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage($"Content must not exceed {ContentMaxLength} characters.");

        this.RuleFor(x => x.Tags)
            .NotNull()
            .WithMessage("Tags collection cannot be null.");

        this.RuleForEach(x => x.Tags)
            .MaximumLength(50)
            .WithMessage("Each tag must not exceed 50 characters.");
    }
}
