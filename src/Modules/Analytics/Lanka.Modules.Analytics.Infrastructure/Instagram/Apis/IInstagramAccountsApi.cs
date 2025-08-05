using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IInstagramAccountsApi
{
    [Get("/{pageId}")]
    Task<InstagramBusinessAccountResponse> GetBusinessAccountAsync(
        [AliasAs("pageId")] string pageId,
        [Query] string fields,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );

    [Get("/{accountId}")]
    Task<InstagramUserInfo> GetUserInfoAsync(
        [AliasAs("accountId")] string accountId,
        [Query] string fields,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );
}
