using System.Globalization;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Application.Instagram.UserActivity;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Posts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Domain.UserActivities;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;
using Lanka.Modules.Analytics.Infrastructure.Posts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramPostService : IInstagramPostService
{
    private readonly IInstagramPostsApi _instagramPostsApi;
    private readonly InstagramPostsRepository _postsRepository;
    private readonly IUserActivityService _userActivityService;

    public InstagramPostService(
        IInstagramPostsApi instagramPostsApi,
        InstagramPostsRepository postsRepository,
        IUserActivityService userActivityService
    )
    {
        this._instagramPostsApi = instagramPostsApi;
        this._postsRepository = postsRepository;
        this._userActivityService = userActivityService;
    }

    public async Task<Result<PostsResponse>> GetUserPostsWithInsights(
        InstagramAccount instagramAccount,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken = default
    )
    {
        InstagramPosts? validData = await this._postsRepository.GetValidAsync(
            instagramAccount.Id.Value,
            cancellationToken
        );

        if (validData is not null)
        {
            return MapCachedToPostsResponse(validData);
        }

        InstagramPosts? fallbackData = await this._postsRepository.GetAsync(
            instagramAccount.Id.Value,
            cancellationToken
        );

        if (instagramAccount.Token is null)
        {
            return fallbackData is null
                ? Result.Failure<PostsResponse>(TokenErrors.NotFound)
                : MapCachedToPostsResponse(fallbackData);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        InstagramMedia? posts = await this.FetchPostsAsync(
            instagramAccount.Metadata.Id,
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

        UserActivityLevel activityLevel = await this._userActivityService
            .GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

        var freshCachedPosts = new InstagramPosts(activityLevel)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            Posts = MapToCachedPosts(posts.Data),
            Paging = MapToPagingInfo(posts.Paging)
        };

        await this._postsRepository.ReplaceAsync(freshCachedPosts, cancellationToken);

        return MapCachedToPostsResponse(freshCachedPosts);
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
                cancellationToken
            )
        );

        await Task.WhenAll(fetchTasks);
    }

    private async Task<InstagramInsightsResponse?> FetchPostInsightsAsync(
        InstagramPostResponse post,
        string accessToken,
        CancellationToken cancellationToken
    )
    {
        InstagramInsightsResponse? insights = await this._instagramPostsApi.GetPostInsightsAsync(
            post.Id,
            metric: "likes,comments,reach",
            accessToken,
            cancellationToken
        );

        return insights;
    }


    private static List<CachedInstagramPost> MapToCachedPosts(List<InstagramPostResponse> posts)
    {
        return posts.ConvertAll(post => new CachedInstagramPost
        {
            Id = post.Id,
            MediaType = post.MediaType,
            MediaUrl = post.MediaUrl,
            Permalink = post.Permalink,
            ThumbnailUrl = post.ThumbnailUrl,
            Timestamp = DateTime.Parse(post.Timestamp, CultureInfo.InvariantCulture),
            Insights = post.Insights?.Data?.ConvertAll(insight => new CachedInsight
            {
                Name = insight.Name,
                Period = insight.Period,
                Values = insight.Values.ConvertAll(v => new CachedInsightValue
                {
                    Value = v.Value
                })
            }) ?? []
        });
    }

    private static InstagramPagingInfo MapToPagingInfo(InstagramPaging paging)
    {
        return new InstagramPagingInfo
        {
            After = paging.Cursors.After,
            Before = paging.Cursors.Before,
            NextCursor = paging.Next,
            PreviousCursor = paging.Previous
        };
    }

    private static PostsResponse MapCachedToPostsResponse(InstagramPosts cachedPosts)
    {
        return new PostsResponse
        {
            Posts = cachedPosts.Posts.ConvertAll(post => new InstagramPost
            {
                Id = post.Id,
                MediaType = post.MediaType,
                MediaUrl = post.MediaUrl,
                Permalink = post.Permalink,
                ThumbnailUrl = post.ThumbnailUrl,
                Timestamp = post.Timestamp,
                Insights = post.Insights.ConvertAll(insight => new InstagramInsight
                {
                    Name = insight.Name,
                    Period = insight.Period,
                    Values = insight.Values.ConvertAll(v => new InstagramInsightValue
                    {
                        Value = v.Value
                    })
                })
            }),
            Paging = new InstagramPagingResponse
            {
                After = cachedPosts.Paging.After,
                Before = cachedPosts.Paging.Before,
                NextCursor = cachedPosts.Paging.NextCursor,
                PreviousCursor = cachedPosts.Paging.PreviousCursor
            }
        };
    }
}
