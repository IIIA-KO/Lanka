using Bogus;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.Posts;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

public static class FakeInstagramDataGenerator
{
    private static readonly Faker Faker = new();

    #region Instagram Account Data

    public static InstagramUserInfo GenerateUserInfo(string? username = null)
    {
#pragma warning disable CA1308 // Instagram usernames are lowercase
        string effectiveUsername = username ?? Faker.Internet.UserName().ToLowerInvariant();
#pragma warning restore CA1308

        return new InstagramUserInfo
        {
            Id = Faker.Random.AlphaNumeric(15),
            FacebookPageId = Faker.Random.AlphaNumeric(15),
            AdAccountId = $"act_{Faker.Random.Number(100000000, 999999999)}",
            BusinessDiscovery = new BusinessDiscovery
            {
                Username = effectiveUsername,
                Name = Faker.Name.FullName(),
                IgId = Faker.Random.Long(100000000, 999999999),
                Id = Faker.Random.AlphaNumeric(15),
                FollowersCount = Faker.Random.Int(1000, 100000),
                MediaCount = Faker.Random.Int(50, 500)
            }
        };
    }

    public static (string FacebookPageId, string AdAccountId, string MetadataId, long MetadataIgId, string UserName, int
        FollowersCount, int MediaCount)
        GenerateAccountMetadata()
    {
#pragma warning disable CA1308 // Instagram usernames are lowercase
        return (
            FacebookPageId: Faker.Random.AlphaNumeric(15),
            AdAccountId: $"act_{Faker.Random.Number(100000000, 999999999)}",
            MetadataId: Faker.Random.AlphaNumeric(15),
            MetadataIgId: Faker.Random.Long(100000000, 999999999),
            UserName: Faker.Internet.UserName().ToLowerInvariant(),
            FollowersCount: Faker.Random.Int(1000, 100000),
            MediaCount: Faker.Random.Int(50, 500)
        );
#pragma warning restore CA1308
    }

    #endregion

    #region Facebook Data

    public static string GenerateFacebookPageId() => Faker.Random.AlphaNumeric(15);

    public static string GenerateAdAccountId() => $"act_{Faker.Random.Number(100000000, 999999999)}";

    public static string GenerateAccessToken() => Faker.Random.AlphaNumeric(200);

    public static (string AccessToken, DateTimeOffset ExpiresAt) GenerateTokenData()
    {
        return (
            AccessToken: GenerateAccessToken(),
            ExpiresAt: DateTimeOffset.UtcNow.AddDays(60)
        );
    }

    public static FacebookTokenResponse GenerateFacebookTokenResponse()
    {
        return new FacebookTokenResponse
        {
            AccessToken = GenerateAccessToken(),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(60)
        };
    }

    public static FacebookUserInfo GenerateFacebookUserInfo()
    {
        return new FacebookUserInfo
        {
            Id = Faker.Random.AlphaNumeric(15),
            Name = Faker.Name.FullName()
        };
    }

    #endregion

    #region Statistics Data

    public static MetricsStatistics GenerateMetricsStatistics(
        Guid instagramAccountId,
        StatisticsPeriod period,
        int daysOfData = 30)
    {
        return new MetricsStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            StatisticsPeriod = period,
            Metrics =
            [
                new TimeSeriesMetricData
                {
                    Name = "impressions",
                    Values = Enumerable.Range(0, daysOfData).ToDictionary(
                        i => DateTime.UtcNow.Date.AddDays(-i),
                        _ => Faker.Random.Int(100, 5000))
                },
                new TimeSeriesMetricData
                {
                    Name = "reach",
                    Values = Enumerable.Range(0, daysOfData).ToDictionary(
                        i => DateTime.UtcNow.Date.AddDays(-i),
                        _ => Faker.Random.Int(50, 4000))
                }
            ]
        };
    }

    public static OverviewStatistics GenerateOverviewStatistics(
        Guid instagramAccountId,
        StatisticsPeriod period)
    {
        return new OverviewStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            StatisticsPeriod = period,
            Metrics =
            [
                new TotalValueMetricData { Name = "followers_count", Value = Faker.Random.Int(1000, 50000) },
                new TotalValueMetricData { Name = "reach", Value = Faker.Random.Int(5000, 100000) },
                new TotalValueMetricData { Name = "impressions", Value = Faker.Random.Int(10000, 200000) }
            ]
        };
    }

    public static InteractionStatistics GenerateInteractionStatistics(
        Guid instagramAccountId,
        StatisticsPeriod period)
    {
        return new InteractionStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            StatisticsPeriod = period,
            EngagementRate = Faker.Random.Double(1, 10),
            AverageLikes = Faker.Random.Double(50, 500),
            AverageComments = Faker.Random.Double(5, 50),
            CPE = Faker.Random.Double(0.1, 2.0)
        };
    }

    public static EngagementStatistics GenerateEngagementStatistics(
        Guid instagramAccountId,
        StatisticsPeriod period)
    {
        return new EngagementStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            StatisticsPeriod = period,
            ReachRate = Faker.Random.Double(10, 50),
            EngagementRate = Faker.Random.Double(1, 5),
            ERReach = Faker.Random.Double(2, 15)
        };
    }

    #endregion

    #region Audience Data

    public static AgePercentage[] GenerateAgeDistribution()
    {
        double remaining = 100.0;
        string[] ranges = ["13-17", "18-24", "25-34", "35-44", "45-54", "55-64", "65+"];
        var distribution = new List<AgePercentage>();

        for (int i = 0; i < ranges.Length - 1; i++)
        {
            double percentage = Math.Round(Faker.Random.Double(5, Math.Min(40, remaining)), 2);
            distribution.Add(new AgePercentage { AgeGroup = ranges[i], Percentage = percentage });
            remaining -= percentage;
        }

        distribution.Add(new AgePercentage { AgeGroup = ranges[^1], Percentage = Math.Round(remaining, 2) });
        return [.. distribution];
    }

    public static GenderPercentage[] GenerateGenderDistribution()
    {
        double femalePercent = Math.Round(Faker.Random.Double(40, 70), 2);
        double malePercent = Math.Round(100 - femalePercent, 2);

        return
        [
            new GenderPercentage { Gender = "F", Percentage = femalePercent },
            new GenderPercentage { Gender = "M", Percentage = malePercent }
        ];
    }

    public static LocationPercentage[] GenerateLocationDistribution(LocationType locationType = LocationType.City)
    {
        string[] locations = locationType == LocationType.City
            ? ["New York", "Los Angeles", "London", "Paris", "Tokyo", "Sydney", "Toronto", "Berlin"]
            : ["United States", "United Kingdom", "Canada", "Australia", "Germany", "France", "Japan", "Brazil"];

        List<string> selected = [.. Faker.PickRandom(locations, 5)];

        double remaining = 100.0;
        var result = new List<LocationPercentage>();

        for (int i = 0; i < selected.Count - 1; i++)
        {
            double percentage = Math.Round(Faker.Random.Double(10, Math.Min(30, remaining)), 2);
            result.Add(new LocationPercentage { Location = selected[i], Percentage = percentage });
            remaining -= percentage;
        }

        result.Add(new LocationPercentage { Location = selected[^1], Percentage = Math.Round(remaining, 2) });
        return [.. result];
    }

    public static (double FollowersPercentage, double NonFollowersPercentage) GenerateReachDistribution()
    {
        double followersPercent = Math.Round(Faker.Random.Double(60, 85), 2);
        return (followersPercent, Math.Round(100 - followersPercent, 2));
    }

    public static GenderDistribution GenerateGenderDistributionDomain(Guid instagramAccountId)
    {
        return new GenderDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            GenderPercentages = GenerateGenderDistribution()
        };
    }

    public static AgeDistribution GenerateAgeDistributionDomain(Guid instagramAccountId)
    {
        return new AgeDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            AgePercentages = GenerateAgeDistribution()
        };
    }

    public static LocationDistribution GenerateLocationDistributionDomain(
        Guid instagramAccountId,
        LocationType locationType = LocationType.City)
    {
        return new LocationDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            LocationType = locationType,
            TopLocationPercentages = GenerateLocationDistribution(locationType)
        };
    }

    public static ReachDistribution GenerateReachDistributionDomain(
        Guid instagramAccountId,
        StatisticsPeriod period)
    {
        (double followers, double nonFollowers) = GenerateReachDistribution();

        return new ReachDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccountId,
            StatisticsPeriod = period,
            TotalReach = Faker.Random.Int(1000, 50000),
            ReachPercentages =
            [
                new ReachPercentage { FollowType = "followers", Percentage = followers },
                new ReachPercentage { FollowType = "non_followers", Percentage = nonFollowers }
            ]
        };
    }

    #endregion

    #region Posts Data

    public static List<CachedInstagramPost> GeneratePosts(int count = 6)
    {
        return [.. Enumerable.Range(0, count).Select(_ => GeneratePost())];
    }

    private static CachedInstagramPost GeneratePost()
    {
        string mediaType = Faker.PickRandom("IMAGE", "VIDEO", "CAROUSEL_ALBUM");
        int likes = Faker.Random.Int(50, 5000);
        int comments = Faker.Random.Int(5, 500);
        int saves = Faker.Random.Int(10, 1000);
        int reach = Faker.Random.Int(likes, likes * 3);
        int impressions = Faker.Random.Int(reach, reach * 2);

        return new CachedInstagramPost
        {
            Id = Faker.Random.AlphaNumeric(15),
            MediaType = mediaType,
            MediaUrl = Faker.Image.PicsumUrl(),
            Permalink = $"https://www.instagram.com/p/{Faker.Random.AlphaNumeric(11)}/",
            ThumbnailUrl = mediaType == "VIDEO" ? Faker.Image.PicsumUrl() : string.Empty,
            Timestamp = Faker.Date.Recent(90),
            Insights =
            [
                new CachedInsight
                    { Name = "likes", Period = "lifetime", Values = [new CachedInsightValue { Value = likes }] },
                new CachedInsight
                    { Name = "comments", Period = "lifetime", Values = [new CachedInsightValue { Value = comments }] },
                new CachedInsight
                    { Name = "saved", Period = "lifetime", Values = [new CachedInsightValue { Value = saves }] },
                new CachedInsight
                    { Name = "reach", Period = "lifetime", Values = [new CachedInsightValue { Value = reach }] },
                new CachedInsight
                {
                    Name = "impressions", Period = "lifetime", Values = [new CachedInsightValue { Value = impressions }]
                }
            ]
        };
    }

    public static InstagramPagingInfo GeneratePagingInfo()
    {
        return new InstagramPagingInfo
        {
            Before = Faker.Random.AlphaNumeric(50),
            After = Faker.Random.AlphaNumeric(50)
        };
    }

    public static PostsResponse GeneratePostsResponse(int limit = 10)
    {
        List<InstagramPost> posts = GenerateInstagramPosts(Math.Min(limit, 10));

        return new PostsResponse
        {
            Posts = posts,
            Paging = new InstagramPagingResponse
            {
                Before = Faker.Random.AlphaNumeric(20),
                After = Faker.Random.AlphaNumeric(20)
            }
        };
    }

    public static List<InstagramPost> GenerateInstagramPosts(int count = 10)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateInstagramPost()).ToList();
    }

    public static InstagramPost GenerateInstagramPost()
    {
        return new InstagramPost
        {
            Id = Faker.Random.AlphaNumeric(20),
            MediaType = Faker.PickRandom("IMAGE", "VIDEO", "CAROUSEL_ALBUM"),
            MediaUrl = Faker.Image.PicsumUrl(),
            Permalink = $"https://www.instagram.com/p/{Faker.Random.AlphaNumeric(11)}/",
            ThumbnailUrl = Faker.Image.PicsumUrl(),
            Timestamp = Faker.Date.Recent(30),
            Insights =
            [
                new InstagramInsight
                {
                    Name = "impressions", Values = [new InstagramInsightValue { Value = Faker.Random.Int(100, 10000) }]
                },
                new InstagramInsight
                    { Name = "reach", Values = [new InstagramInsightValue { Value = Faker.Random.Int(50, 8000) }] },
                new InstagramInsight
                    { Name = "engagement", Values = [new InstagramInsightValue { Value = Faker.Random.Int(10, 500) }] },
                new InstagramInsight
                    { Name = "saved", Values = [new InstagramInsightValue { Value = Faker.Random.Int(1, 100) }] },
                new InstagramInsight
                {
                    Name = "video_views", Values = [new InstagramInsightValue { Value = Faker.Random.Int(100, 5000) }]
                }
            ]
        };
    }

    #endregion
}
