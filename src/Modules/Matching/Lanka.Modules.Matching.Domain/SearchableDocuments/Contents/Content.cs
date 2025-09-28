using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.SearchableDocuments.Contents;

public sealed record Content
{
    public const int MaxLength = 10000;
    
    public string Value { get; init; }

    private Content(string value)
    {
        this.Value = value;
    }

    public static Result<Content> Create(string content)
    {
        Result validationResult = ValidateContentString(content);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Content>(validationResult.Error);
        }

        return new Content(content.Trim());
    }

    private static Result ValidateContentString(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ContentErrors.Empty;
        }

        if (content.Length > MaxLength)
        {
            return ContentErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}


