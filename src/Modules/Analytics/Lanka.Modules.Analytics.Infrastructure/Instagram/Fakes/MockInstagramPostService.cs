using Bogus;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramPostService : IInstagramPostService
{
    private readonly Faker _faker = new();

    public Task<Result<PostsResponse>> GetUserPostsWithInsights(
        InstagramAccount instagramAccount,
        int limit,
        string cursorType,
        string cursor,
        CancellationToken cancellationToken = default)
    {
        // Generate realistic Instagram posts with insights
        var posts = new List<InstagramPost>();

        for (int i = 0; i < Math.Min(limit, 10); i++)
        {
            var post = new InstagramPost
            {
                Id = this._faker.Random.AlphaNumeric(20),
                MediaType = this._faker.PickRandom("IMAGE", "VIDEO", "CAROUSEL_ALBUM"),
                MediaUrl = this._faker.Internet.Url(),
                Permalink = this._faker.Internet.Url(),
                ThumbnailUrl = this._faker.Internet.Url(),
                Timestamp = this._faker.Date.Recent(30),
                Insights =
                [
                    new InstagramInsight { Name = "impressions", Values = [ new InstagramInsightValue { Value = this._faker.Random.Int(100, 10000) } ] },
                    new InstagramInsight { Name = "reach", Values = [ new InstagramInsightValue { Value = this._faker.Random.Int(50, 8000) } ] },
                    new InstagramInsight { Name = "engagement", Values = [ new InstagramInsightValue { Value = this._faker.Random.Int(10, 500) } ] },
                    new InstagramInsight { Name = "saved", Values = [ new InstagramInsightValue { Value = this._faker.Random.Int(1, 100) } ] },
                    new InstagramInsight { Name = "video_views", Values = [ new InstagramInsightValue { Value = this._faker.Random.Int(100, 5000) } ] }
                ]
            };

            posts.Add(post);
        }

        var response = new PostsResponse
        {
            Posts = posts,
            Paging = new InstagramPagingResponse
            {
                Before = this._faker.Random.AlphaNumeric(20),
                After = this._faker.Random.AlphaNumeric(20)
            }
        };

        return Task.FromResult((Result<PostsResponse>)response);
    }
}
