using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.GetReview;

internal sealed class GetReviewQueryHandler
    : IQueryHandler<GetReviewQuery, ReviewResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetReviewQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<ReviewResponse>> Handle(GetReviewQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 id AS {nameof(ReviewResponse.Id)},
                 client_id AS {nameof(ReviewResponse.ClientId)},
                 creator_id AS {nameof(ReviewResponse.CreatorId)},
                 offer_id AS {nameof(ReviewResponse.OfferId)},
                 campaign_id AS {nameof(ReviewResponse.CampaignId)},
                 rating AS {nameof(ReviewResponse.Rating)},
                 comment AS {nameof(ReviewResponse.Comment)},
                 crated_on_utc AS {nameof(ReviewResponse.CreatedOnUtc)}
             FROM campaigns.reviews
             WHERE id = @ReviewId
             """;

        ReviewResponse? review = await connection.QuerySingleOrDefaultAsync<ReviewResponse>(
            sql,
            new { request.ReviewId }
        );
        
        return review ?? Result.Failure<ReviewResponse>(ReviewErrors.NotFound);
    }
}
