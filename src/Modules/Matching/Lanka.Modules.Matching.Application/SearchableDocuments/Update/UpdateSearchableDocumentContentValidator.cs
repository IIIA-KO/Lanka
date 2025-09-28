using FluentValidation;
using Lanka.Modules.Matching.Domain.SearchableDocuments.Contents;
using Lanka.Modules.Matching.Domain.SearchableDocuments.Titles;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Update;

internal sealed class
    UpdateSearchableDocumentContentValidator : AbstractValidator<UpdateSearchableDocumentContentCommand>
{
    public UpdateSearchableDocumentContentValidator()
    {
        this.RuleFor(x => x.SourceEntityId)
            .NotEmpty()
            .WithMessage("Source entity id is required.");

        this.RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid SearchableItemType.");

        this.RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(Title.MaxLength)
            .WithMessage($"Title cannot exceed {Title.MaxLength} characters.");

        this.RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required.")
            .MaximumLength(Content.MaxLength)
            .WithMessage($"Content cannot exceed {Content.MaxLength} characters.");

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
