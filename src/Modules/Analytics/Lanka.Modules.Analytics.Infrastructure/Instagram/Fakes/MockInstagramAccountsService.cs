using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramAccountsService : IInstagramAccountsService
{
    public Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        string facebookPageId,
        string instagramUsername,
        CancellationToken cancellationToken = default
    )
    {
        InstagramUserInfo userInfo = FakeInstagramDataGenerator.GenerateUserInfo(instagramUsername);
        return Task.FromResult((Result<InstagramUserInfo>)userInfo);
    }

    public Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        InstagramUserInfo userInfo = FakeInstagramDataGenerator.GenerateUserInfo();
        return Task.FromResult((Result<InstagramUserInfo>)userInfo);
    }
}
