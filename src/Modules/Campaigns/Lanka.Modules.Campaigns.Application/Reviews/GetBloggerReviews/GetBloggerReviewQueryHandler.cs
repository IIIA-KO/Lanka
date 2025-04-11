using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;

namespace Lanka.Modules.Campaigns.Application.Reviews.GetBloggerReviews;

internal sealed class GetBloggerReviewQueryHandler
    : IQueryHandler<GetBloggerReviewsQuery, IReadOnlyList<ReviewResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetBloggerReviewQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<ReviewResponse>>> Handle(GetBloggerReviewsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 id AS {nameof(ReviewResponse.Id)},
                 rating AS {nameof(ReviewResponse.Rating)},
                 comment AS {nameof(ReviewResponse.Comment)},
                 crated_on_utc AS {nameof(ReviewResponse.CreatedOnUtc)}
             FROM campaigns.reviews
             WHERE creator_id = @BloggerId
             """;

        IEnumerable<ReviewResponse> reviews = await connection.QueryAsync<ReviewResponse>(
            sql,
            new { request.BloggerId }
        );

        return reviews.ToList();
    }
}
