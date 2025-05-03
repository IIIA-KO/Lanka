using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Emails;

public sealed record Email
{
    public const int MaxLength = 255;
        
    public string Value { get; init; }

    private Email(string value)
    {
        this.Value = value;
    }

    public static Result<Email> Create(string email)
    {
        Result validationResult = ValidateEmailString(email);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Email>(validationResult.Error);
        }
            
        return new Email(email);
    }

    private static Result ValidateEmailString(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return EmailErrors.Empty;
        }

        if (email.Length > MaxLength)
        {
            return EmailErrors.TooLonng(MaxLength);
        }
            
        return Result.Success();
    }
}
