using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IInstagramTokenApi
{
    [Post("/oauth/access_token")]
    Task<FacebookTokenResponse> GetAccessTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> formData,
        CancellationToken cancellationToken = default
    );

    [Get("/debug_token")]
    Task<DebugTokenResponse> GetDebugTokenAsync(
        [AliasAs("input_token")] string inputToken,
        [AliasAs("access_token")] string accessToken,
        CancellationToken cancellationToken = default
    );
}
