using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Offers.Names;

public sealed record Name
{
    public const int MaxLength = 100;
        
    public string Value { get; init; }

    private Name(string value)
    {
        this.Value = value;
    }

    public static Result<Name> Create(string name)
    {
        Result validationResult = ValidateNameString(name);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Name>(validationResult.Error);
        }

        return new Name(name);
    }

    private static Result ValidateNameString(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return NameErrors.Empty;
        }

        if (name.Length > MaxLength)
        {
            return NameErrors.TooLong(MaxLength);
        }
            
        return Result.Success();
    }
}
