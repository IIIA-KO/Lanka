using Bogus;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockFacebookService : IFacebookService
{
    private readonly Faker _faker = new();

    public Task<Result<string>> GetFacebookPageIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        string pageId = this._faker.Random.AlphaNumeric(15);
        return Task.FromResult((Result<string>)pageId);
    }

    public Task<Result<string>> GetAdAccountIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        string adAccountId = $"act_{this._faker.Random.Number(100000000, 999999999)}";
        return Task.FromResult((Result<string>)adAccountId);
    }

    public Task<Result<FacebookTokenResponse>> GetAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResponse = new FacebookTokenResponse
        {
            AccessToken = this._faker.Random.AlphaNumeric(200),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(60)
        };

        return Task.FromResult((Result<FacebookTokenResponse>)tokenResponse);
    }

    public Task<Result<FacebookTokenResponse>> RenewAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return this.GetAccessTokenAsync(code, cancellationToken);
    }

    public Task<FacebookUserInfo?> GetFacebookUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        var userInfo = new FacebookUserInfo
        {
            Id = this._faker.Random.AlphaNumeric(15),
            Name = this._faker.Name.FullName()
        };

        return Task.FromResult<FacebookUserInfo?>(userInfo);
    }
}
