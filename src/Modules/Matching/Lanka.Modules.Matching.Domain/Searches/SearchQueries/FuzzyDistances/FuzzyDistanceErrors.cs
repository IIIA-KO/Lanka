using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.FuzzyDistances;

public static class FuzzyDistanceErrors
{
    public static Error Invalid(double min, double max) =>
        Error.Validation(
            "FuzzyDistance.Invalid",
            $"Fuzzy distance must be between {min} and {max}."
        );
}
