using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;

namespace Lanka.Modules.Analytics.Application.Instagram.GetPosts;

public sealed record GetPostsQuery(Guid UserId, int Limit, string? CursorType, string? Cursor)
    : ICachedQuery<PostsResponse>
{
    public string CacheKey =>
        $"posts-{this.UserId}-limit-{this.Limit}-cursorType-{this.CursorType ?? "after"}-cursor-{this.Cursor ?? "null"}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
}
