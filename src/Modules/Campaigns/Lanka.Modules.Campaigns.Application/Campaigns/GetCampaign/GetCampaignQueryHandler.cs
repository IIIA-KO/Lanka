using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;

internal sealed class GetCampaignQueryHandler
    : IQueryHandler<GetCampaignQuery, CampaignResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetCampaignQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<CampaignResponse>> Handle(GetCampaignQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
              SELECT
                 c.id AS {nameof(CampaignResponse.Id)},
                 CASE c.status
                    WHEN 0 THEN 'Pending'
                    WHEN 1 THEN 'Confirmed'
                    WHEN 2 THEN 'Rejected'
                    WHEN 3 THEN 'Cancelled'
                    WHEN 4 THEN 'Done'
                    WHEN 5 THEN 'Completed'
                 END AS {nameof(CampaignResponse.Status)},
                 c.name AS {nameof(CampaignResponse.Name)},
                 c.description AS {nameof(CampaignResponse.Description)},
                 c.offer_id AS {nameof(CampaignResponse.OfferId)},
                 c.client_id AS {nameof(CampaignResponse.ClientId)},
                 c.creator_id AS {nameof(CampaignResponse.CreatorId)},
                 c.scheduled_on_utc AS {nameof(CampaignResponse.ScheduledOnUtc)},
                 c.pended_on_utc AS {nameof(CampaignResponse.PendedOnUtc)},
                 c.confirmed_on_utc AS {nameof(CampaignResponse.ConfirmedOnUtc)},
                 c.rejected_on_utc AS {nameof(CampaignResponse.RejectedOnUtc)},
                 c.cancelled_on_utc AS {nameof(CampaignResponse.CancelledOnUtc)},
                 c.done_on_utc AS {nameof(CampaignResponse.DoneOnUtc)},
                 c.completed_on_utc AS {nameof(CampaignResponse.CompletedOnUtc)},
                 c.price_amount AS {nameof(CampaignResponse.PriceAmount)},
                 c.price_currency AS {nameof(CampaignResponse.PriceCurrency)},
                 bc.first_name AS {nameof(CampaignResponse.CreatorFirstName)},
                 bc.last_name AS {nameof(CampaignResponse.CreatorLastName)},
                 bl.first_name AS {nameof(CampaignResponse.ClientFirstName)},
                 bl.last_name AS {nameof(CampaignResponse.ClientLastName)}
              FROM campaigns.campaigns c
              INNER JOIN campaigns.bloggers bc ON c.creator_id = bc.id
              INNER JOIN campaigns.bloggers bl ON c.client_id = bl.id
              WHERE c.id = @CampaignId
             """;
        CampaignResponse campaign = await connection.QuerySingleOrDefaultAsync<CampaignResponse>(
            sql,
            new { request.CampaignId }
        );

        return campaign ?? Result.Failure<CampaignResponse>(CampaignErrors.NotFound);
    }
}
