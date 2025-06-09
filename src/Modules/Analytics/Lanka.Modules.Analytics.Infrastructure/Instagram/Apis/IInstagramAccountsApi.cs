using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IInstagramAccountsApi
{
    [Get("/{pageId}")]
    Task<InstagramBusinessAccountResponse> GetBusinessAccountAsync(
        [AliasAs("pageId")] string pageId,
        [Query] IDictionary<string, string> query,
        CancellationToken cancellationToken = default
    );

    [Get("/{accountId}")]
    Task<InstagramUserInfo> GetUserInfoAsync(
        [AliasAs("accountId")] string accountId,
        [Query] IDictionary<string, string> query,
        CancellationToken cancellationToken = default
    );
}
