using Bogus;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramAudienceService : IInstagramAudienceService
{
    private readonly Faker _faker = new();

    public Task<Result<GenderDistribution>> GetAudienceGenderPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default)
    {
        // Generate realistic gender distribution
        double malePercentage = this._faker.Random.Double(30, 70);
        double femalePercentage = 100 - malePercentage;

        var distribution = new GenderDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            GenderPercentages =
            [
                new GenderPercentage { Gender = "male", Percentage = Math.Round(malePercentage, 2) },
                new GenderPercentage { Gender = "female", Percentage = Math.Round(femalePercentage, 2) }
            ]
        };

        return Task.FromResult((Result<GenderDistribution>)distribution);
    }

    public Task<Result<LocationDistribution>> GetAudienceTopLocations(
        InstagramAccount instagramAccount,
        LocationType locationType,
        CancellationToken cancellationToken = default)
    {
        // Generate top 5 locations with realistic percentages
        var locations = new Dictionary<string, double>();
        double remainingPercentage = 100.0;

        for (int i = 0; i < 5; i++)
        {
            string? location = locationType == LocationType.City
                ? this._faker.Address.City()
                : this._faker.Address.Country();

            double percentage = i == 4
                ? remainingPercentage
                : this._faker.Random.Double(5, remainingPercentage / 2);

            locations[location] = Math.Round(percentage, 2);
            remainingPercentage -= percentage;
        }

        var distribution = new LocationDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            LocationType = locationType,
            TopLocationPercentages = [.. locations.Select(l => new LocationPercentage { Location = l.Key, Percentage = l.Value })]
        };

        return Task.FromResult((Result<LocationDistribution>)distribution);
    }

    public Task<Result<AgeDistribution>> GetAudienceAgesPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default)
    {
        // Generate realistic age distribution
        var ageRanges = new Dictionary<string, double>
        {
            ["13-17"] = this._faker.Random.Double(5, 15),
            ["18-24"] = this._faker.Random.Double(20, 35),
            ["25-34"] = this._faker.Random.Double(25, 40),
            ["35-44"] = this._faker.Random.Double(10, 20),
            ["45-54"] = this._faker.Random.Double(5, 15),
            ["55-64"] = this._faker.Random.Double(2, 10),
            ["65+"] = this._faker.Random.Double(1, 5)
        };

        // Normalize to 100%
        double total = ageRanges.Values.Sum();
        var normalized = ageRanges.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Round((kvp.Value / total) * 100, 2)
        );

        var distribution = new AgeDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            AgePercentages = [.. normalized.Select(a => new AgePercentage { AgeGroup = a.Key, Percentage = a.Value })]
        };

        return Task.FromResult((Result<AgeDistribution>)distribution);
    }

    public Task<Result<ReachDistribution>> GetAudienceReachPercentage(
        InstagramAccount instagramAccount,
        StatisticsPeriod period,
        CancellationToken cancellationToken = default)
    {
        // Generate realistic reach distribution (followers vs non-followers)
        double followersPercentage = this._faker.Random.Double(60, 85);
        double nonFollowersPercentage = 100 - followersPercentage;

        var distribution = new ReachDistribution(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            StatisticsPeriod = period,
            TotalReach = this._faker.Random.Int(1000, 50000),
            ReachPercentages =
            [
                new ReachPercentage { FollowType = "followers", Percentage = Math.Round(followersPercentage, 2) },
                new ReachPercentage { FollowType = "non_followers", Percentage = Math.Round(nonFollowersPercentage, 2) }
            ]
        };

        return Task.FromResult((Result<ReachDistribution>)distribution);
    }
}
