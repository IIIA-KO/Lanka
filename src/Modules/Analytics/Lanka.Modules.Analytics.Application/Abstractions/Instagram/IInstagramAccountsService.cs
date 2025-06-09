using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramAccountsService
{
    Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        string facebookPageId,
        string instagramUsername,
        CancellationToken cancellationToken = default
    );

    Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );
}
