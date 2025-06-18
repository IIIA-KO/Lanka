using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramPostService
{
    Task<Result<PostsResponse>> GetUserPostsWithInsights(
        string accessToken,
        string instagramAccountId,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken = default
    );
}
