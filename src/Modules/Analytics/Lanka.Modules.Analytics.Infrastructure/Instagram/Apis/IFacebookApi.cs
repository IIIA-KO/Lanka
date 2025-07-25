using System.Text.Json;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models;
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
        [Query] Dictionary<string, string> queryParams,
        CancellationToken cancellationToken = default
    );

    [Get("/{userId}/adaccounts")]
    Task<JsonElement> GetAdAccountsAsync(
        [AliasAs("userId")] string userId,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );
    
    [Post("/oauth/access_token")]
    Task<FacebookTokenResponse> GetAccessTokenAsync(
        FormUrlEncodedContent formData,
        CancellationToken cancellationToken = default
    );
    
    [Get("/debug_token")]
    Task<DebugTokenResponse>  GetDebugTokenAsync(
        [AliasAs("input_token")] string inputToken,
        [AliasAs("access_token")] string accessToken,
        CancellationToken cancellationToken = default
    );
}
