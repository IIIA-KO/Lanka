using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Instagram;

public sealed record UpdateInstagramDataCommand(
    Guid UserId,
    string Username,
    int FollowersCount,
    int MediaCount,
    double? EngagementRate = null,
    string? AudienceTopAgeGroup = null,
    string? AudienceTopGender = null,
    double? AudienceTopGenderPercentage = null,
    string? AudienceTopCountry = null,
    double? AudienceTopCountryPercentage = null) : ICommand;
