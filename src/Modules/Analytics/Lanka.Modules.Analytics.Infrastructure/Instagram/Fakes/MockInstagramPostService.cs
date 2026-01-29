using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramPostService : IInstagramPostService
{
    public Task<Result<PostsResponse>> GetUserPostsWithInsights(
        InstagramAccount instagramAccount,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken = default)
    {
        PostsResponse response = FakeInstagramDataGenerator.GeneratePostsResponse(limit);
        return Task.FromResult((Result<PostsResponse>)response);
    }
}
