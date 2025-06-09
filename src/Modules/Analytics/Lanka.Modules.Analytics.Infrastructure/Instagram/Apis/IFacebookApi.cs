using System.Text.Json;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IFacebookApi
{
    [Get("/me")]
    Task<FacebookUserInfo?> GetUserInfoAsync(
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );

    [Get("/{userId}/accounts")]
    Task<FacebookAccountsResponse?> GetAccountsAsync(
        [AliasAs("userId")] string userId,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );

    [Get("/{userId}/adaccounts")]
    Task<JsonElement> GetAdAccountsAsync(
        [AliasAs("userId")] string userId,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );
}
