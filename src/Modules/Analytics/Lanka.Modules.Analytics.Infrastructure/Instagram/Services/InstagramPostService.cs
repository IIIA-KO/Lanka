using System.Globalization;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramPostService : IInstagramPostService
{
    private readonly IInstagramPostsApi _instagramPostsApi;

    public InstagramPostService(IInstagramPostsApi instagramPostsApi)
    {
        this._instagramPostsApi = instagramPostsApi;
    }

    public async Task<Result<PostsResponse>> GetUserPostsWithInsights(
        string accessToken,
        string instagramAccountId,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken = default
    )
    {
        InstagramMedia? posts = await this.FetchPostsAsync(
            instagramAccountId,
            accessToken,
            limit,
            cursorType,
            cursor,
            cancellationToken
        );

        if (posts?.Data == null)
        {
            return new PostsResponse { Posts = [] };
        }

        await this.FetchInsightsForPostsAsync(posts.Data, accessToken, cancellationToken);
        return this.MapToUserPostsResponse(posts);
    }

    private async Task<InstagramMedia?> FetchPostsAsync(
        string instagramAccountId,
        string accessToken,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken
    )
    {
        const string fields = "id,media_type,media_url,permalink,thumbnail_url,timestamp";
        string? after = cursorType == "after" ? cursor : null;
        string? before = cursorType == "before" ? cursor : null;

        return await this._instagramPostsApi.GetMediaAsync(
            instagramAccountId,
            fields,
            limit,
            after,
            before,
            accessToken,
            cancellationToken
        );
    }

    private async Task FetchInsightsForPostsAsync(
        List<InstagramPostResponse> posts,
        string accessToken,
        CancellationToken cancellationToken
    )
    {
        IEnumerable<Task<InstagramInsightsResponse?>> fetchTasks = posts.Select(async post =>
            post.Insights = await this.FetchPostInsightsAsync(
                post,
                accessToken,
                post.MediaType,
                cancellationToken
            )
        );

        await Task.WhenAll(fetchTasks);
    }

    private async Task<InstagramInsightsResponse?> FetchPostInsightsAsync(
        InstagramPostResponse post,
        string accessToken,
        string mediaType,
        CancellationToken cancellationToken
    )
    {
        string metrics = mediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
            ? "likes,saved,video_views"
            : "likes,saved";

        InstagramInsightsResponse? insights = await this._instagramPostsApi.GetPostInsightsAsync(
            post.Id,
            metrics,
            accessToken,
            cancellationToken
        );

        if (!mediaType.Equals("video", StringComparison.OrdinalIgnoreCase) && insights is not null)
        {
            RemoveVideoViewsInsight(insights);
        }

        return insights;
    }

    private static void RemoveVideoViewsInsight(InstagramInsightsResponse insights)
    {
        InstagramInsightResponse? videoViews = insights.Data?.Find(i => i.Name == "video_views");
        videoViews?.Values.ForEach(v => v.Value = null);
    }

    private PostsResponse MapToUserPostsResponse(InstagramMedia posts)
    {
        return new PostsResponse
        {
            Posts = posts.Data.ConvertAll(post => new InstagramPost
            {
                Id = post.Id,
                MediaType = post.MediaType,
                MediaUrl = post.MediaUrl,
                Permalink = post.Permalink,
                ThumbnailUrl = post.ThumbnailUrl,
                Timestamp = DateTime.Parse(post.Timestamp, CultureInfo.InvariantCulture),
                Insights = post.Insights?.Data?.ConvertAll(insight => new InstagramInsight
                {
                    Name = insight.Name,
                    Value = insight.Values.FirstOrDefault()?.Value
                }) ?? []
            }),
            Paging = new InstagramPagingResponse
            {
                After = posts.Paging.Cursors.After,
                Before = posts.Paging.Cursors.Before,
                NextCursor = posts.Paging.Next,
                PreviousCursor = posts.Paging.Previous
            }
        };
    }
}
