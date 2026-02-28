namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public record InstagramMetadata(
    string? Username,
    int? FollowersCount,
    int? MediaCount,
    double? EngagementRate = null,
    string? AudienceTopAgeGroup = null,
    string? AudienceTopGender = null,
    double? AudienceTopGenderPercentage = null,
    string? AudienceTopCountry = null,
    double? AudienceTopCountryPercentage = null);
