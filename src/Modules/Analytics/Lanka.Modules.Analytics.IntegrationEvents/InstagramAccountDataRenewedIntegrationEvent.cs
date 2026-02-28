using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Analytics.IntegrationEvents;

public sealed class InstagramAccountDataRenewedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountDataRenewedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string username,
        int followersCount,
        int mediaCount,
        string providerId,
        double? engagementRate = null,
        string? audienceTopAgeGroup = null,
        string? audienceTopGender = null,
        double? audienceTopGenderPercentage = null,
        string? audienceTopCountry = null,
        double? audienceTopCountryPercentage = null
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Username = username;
        this.FollowersCount = followersCount;
        this.MediaCount = mediaCount;
        this.ProviderId = providerId;
        this.EngagementRate = engagementRate;
        this.AudienceTopAgeGroup = audienceTopAgeGroup;
        this.AudienceTopGender = audienceTopGender;
        this.AudienceTopGenderPercentage = audienceTopGenderPercentage;
        this.AudienceTopCountry = audienceTopCountry;
        this.AudienceTopCountryPercentage = audienceTopCountryPercentage;
    }

    public Guid UserId { get; }

    public string Username { get; }

    public int FollowersCount { get; init; }

    public int MediaCount { get; init; }

    public string ProviderId { get; }

    public double? EngagementRate { get; init; }

    public string? AudienceTopAgeGroup { get; init; }

    public string? AudienceTopGender { get; init; }

    public double? AudienceTopGenderPercentage { get; init; }

    public string? AudienceTopCountry { get; init; }

    public double? AudienceTopCountryPercentage { get; init; }
}
