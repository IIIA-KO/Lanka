using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.SearchableDocuments.Titles;

public sealed record Title
{
    public const int MaxLength = 500;
    
    public string Value { get; init; }

    private Title(string value)
    {
        this.Value = value;
    }

    public static Result<Title> Create(string title)
    {
        Result validationResult = ValidateTitleString(title);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Title>(validationResult.Error);
        }

        return new Title(title.Trim());
    }

    private static Result ValidateTitleString(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return TitleErrors.Empty;
        }

        if (title.Length > MaxLength)
        {
            return TitleErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}


