using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Pacts.Contents;

public sealed record Content
{
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

        return new Content(content);
    }
        
    private static Result ValidateContentString(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ContentErrors.Empty;
        }

        return Result.Success();
    }
}
