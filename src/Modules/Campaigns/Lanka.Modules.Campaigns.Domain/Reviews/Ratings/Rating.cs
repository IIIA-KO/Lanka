using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews.Ratings;

public sealed record Rating
{
    public static readonly int MaxValue = 5;
    
    public static readonly int MinValue = 1;

    public int Value { get; private set; }

    private Rating(int value)
    {
        this.Value = value;
    }

    public static Result<Rating> Create(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            return Result.Failure<Rating>(RatingErrors.Invalid);
        }

        return new Rating(value);
    }
}
