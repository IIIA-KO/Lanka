using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.SearchTexts;

public sealed record SearchText
{
    public const int MaxLength = 500;
    public string Value { get; init; }

    private SearchText(string value)
    {
        this.Value = value;
    }

    public static Result<SearchText> Create(string text)
    {
        Result validationResult = ValidateText(text);

        if (validationResult.IsFailure)
        {
            return Result.Failure<SearchText>(validationResult.Error);
        }

        return new SearchText(text.Trim());
    }

    private static Result ValidateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return SearchTextErrors.EmptyText;
        }

        if (text.Length > MaxLength)
        {
            return SearchTextErrors.TextTooLong(MaxLength);
        }

        return Result.Success();
    }
}
