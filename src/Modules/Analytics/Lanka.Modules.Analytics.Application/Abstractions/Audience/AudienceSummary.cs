namespace Lanka.Modules.Analytics.Application.Abstractions.Audience;

public sealed record AudienceSummary(
    double? EngagementRate,
    string? TopAgeGroup,
    string? TopGender,
    double? TopGenderPercentage,
    string? TopCountry,
    double? TopCountryPercentage
);
