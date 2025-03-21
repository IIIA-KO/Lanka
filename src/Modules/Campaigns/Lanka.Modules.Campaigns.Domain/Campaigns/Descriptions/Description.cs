using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.Descriptions;

public sealed record Description
{
    public const int MaxLength = 250;
        
    public string Value { get; init; }

    private Description(string value)
    {
        this.Value = value;
    }

    public static Result<Description> Create(string description)
    {
        Result validationResult = ValideDescriptionString(description);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Description>(validationResult.Error);
        }
            
        return new Description(description);
    }

    private static Result ValideDescriptionString(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return DescriptionErrors.Empty;
        }

        if (description.Length > MaxLength)
        {
            return DescriptionErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}
