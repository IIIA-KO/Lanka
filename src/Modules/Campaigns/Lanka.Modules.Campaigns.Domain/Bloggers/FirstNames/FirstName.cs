using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.FirstNames;

public sealed record FirstName
{
    public const int MaxLength = 100;

    public string Value { get; init; }

    private FirstName(string value)
    {
        this.Value = value;
    }

    public static Result<FirstName> Create(string firstName)
    {
        Result validationResult = ValidateFirstNameString(firstName);

        if (validationResult.IsFailure)
        {
            return Result.Failure<FirstName>(validationResult.Error);
        }
            
        return new FirstName(firstName);
    }

    private static Result ValidateFirstNameString(string? firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return FirstNameErrors.Empty;
        }

        if (firstName.Length > MaxLength)
        {
            return FirstNameErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}
