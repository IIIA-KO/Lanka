using Bogus;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramAccountsService : IInstagramAccountsService
{
    private readonly Faker _faker = new();

    public Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        string facebookPageId,
        string instagramUsername,
        CancellationToken cancellationToken = default
    )
    {
        var userInfo = new InstagramUserInfo
        {
            Id = this._faker.Random.AlphaNumeric(15),
            FacebookPageId = this._faker.Random.AlphaNumeric(15),
            AdAccountId = $"act_{this._faker.Random.Number(100000000, 999999999)}",
            BusinessDiscovery = new BusinessDiscovery
            {
                Username = instagramUsername,
                Name = this._faker.Name.FullName(),
                IgId = this._faker.Random.Long(100000000, 999999999),
                Id = this._faker.Random.AlphaNumeric(15),
                FollowersCount = this._faker.Random.Int(100, 50000), // Ensure >= 100
                MediaCount = this._faker.Random.Int(10, 500)
            }
        };

        return Task.FromResult((Result<InstagramUserInfo>)userInfo);
    }

    public Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
#pragma warning disable CA1308 // Normalize strings to uppercase
        string username = this._faker.Internet.UserName().ToLowerInvariant();
#pragma warning restore CA1308
        return this.GetUserInfoAsync(accessToken, string.Empty, username, cancellationToken);
    }
}
