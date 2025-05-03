using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.BirthDates;

public class BirthDate
{
    public const int MinimumAge = 18;
        
    public DateOnly Value { get; init; }

    private BirthDate(DateOnly value)
    {
        this.Value = value;
    }

    public static Result<BirthDate> Create(DateOnly birthDate)
    {
        Result validationResult = ValidateBirthDate(birthDate);

        if (validationResult.IsFailure)
        {
            return Result.Failure<BirthDate>(validationResult.Error);
        }
            
        return new BirthDate(birthDate);
    }
        
    private static Result ValidateBirthDate(DateOnly birthDate)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
            
        var birthDateTimeOffset = new DateTimeOffset(
            birthDate.ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero
        );
            
        int age = now.Year - birthDateTimeOffset.Year;

        if (now < birthDateTimeOffset.AddYears(age))
        {
            age--;
        }

        return age >= MinimumAge
            ? Result.Success() 
            : BirthDateErrors.Invalid;
    }
}
