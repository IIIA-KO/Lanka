using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.LastNames;

public sealed record LastName 
{
    public const int MaxLength = 100;

    public string Value { get; init; }

    private LastName(string value)
    {
        this.Value = value;
    }

    public static Result<LastName> Create(string lastName)
    {
        Result validationResult = ValidateLastNameString(lastName);

        if (validationResult.IsFailure)
        {
            return Result.Failure<LastName>(validationResult.Error);
        }
            
        return new LastName(lastName);
    }

    private static Result ValidateLastNameString(string? lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return LastNameErrors.Empty;
        }

        if (lastName.Length > MaxLength)
        {
            return LastNameErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}
