using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IInstagramPostsApi
{
    [Get("/{instagramAccountId}/media")]
    Task<InstagramMedia?> GetMediaAsync(
        [AliasAs("instagramAccountId")] string instagramAccountId,
        [Query] string fields,
        [Query] int limit,
        [Query] string? after,
        [Query] string? before,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );

    [Get("/{postId}/insights")]
    Task<InstagramInsightsResponse?> GetPostInsightsAsync(
        [AliasAs("postId")] string postId,
        [Query] string metric,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );
}
