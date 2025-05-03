using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Bios;

public record Bio
{
    public const int MaxLength = 150;
        
    public string Value { get; init; }

    private Bio(string value)
    {
        this.Value = value;
    }
    
    public static Result<Bio> Create(string bio)
    {
        ArgumentNullException.ThrowIfNull(bio);

        Result validationResult = ValidateBioString(bio);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Bio>(validationResult.Error);
        }

        return new Bio(bio);
    }

    private static Result ValidateBioString(string bio)
    {
        if (bio.Length > MaxLength)
        {
            return BioErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}
