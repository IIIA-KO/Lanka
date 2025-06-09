namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;

public sealed class PostsResponse
{
    public List<InstagramPost> Posts { get; init; }

    public InstagramPagingResponse Paging { get; init; }
}
