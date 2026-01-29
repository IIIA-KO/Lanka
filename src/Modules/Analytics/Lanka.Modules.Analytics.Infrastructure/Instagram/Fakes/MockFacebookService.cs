using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockFacebookService : IFacebookService
{
    public Task<Result<string>> GetFacebookPageIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        string pageId = FakeInstagramDataGenerator.GenerateFacebookPageId();
        return Task.FromResult((Result<string>)pageId);
    }

    public Task<Result<string>> GetAdAccountIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        string adAccountId = FakeInstagramDataGenerator.GenerateAdAccountId();
        return Task.FromResult((Result<string>)adAccountId);
    }

    public Task<Result<FacebookTokenResponse>> GetAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        FacebookTokenResponse tokenResponse = FakeInstagramDataGenerator.GenerateFacebookTokenResponse();
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
        FacebookUserInfo userInfo = FakeInstagramDataGenerator.GenerateFacebookUserInfo();
        return Task.FromResult<FacebookUserInfo?>(userInfo);
    }
}
