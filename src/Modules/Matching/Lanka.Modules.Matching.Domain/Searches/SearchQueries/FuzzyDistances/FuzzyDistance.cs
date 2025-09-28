using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.FuzzyDistances;

public sealed record FuzzyDistance
{
    public const double Min = 0.0;
    public const double Max = 1.0;
    public double Value { get; init; }

    private FuzzyDistance(double value)
    {
        this.Value = value;
    }

    public static Result<FuzzyDistance> Create(double value)
    {
        Result validationResult = ValidateFuzzyDistance(value);

        if (validationResult.IsFailure)
        {
            return Result.Failure<FuzzyDistance>(validationResult.Error);
        }

        return new FuzzyDistance(value);
    }

    private static Result ValidateFuzzyDistance(double value)
    {
        if (value is < Min or > Max)
        {
            return FuzzyDistanceErrors.Invalid(Min, Max);
        }

        return Result.Success();
    }
}

