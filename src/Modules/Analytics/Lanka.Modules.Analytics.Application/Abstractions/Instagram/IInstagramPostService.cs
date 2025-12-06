using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramPostService
{
    Task<Result<PostsResponse>> GetUserPostsWithInsights(
        InstagramAccount instagramAccount,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken = default
    );
}
