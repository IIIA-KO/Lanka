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
                 id AS {nameof(CampaignResponse.Id)},
                 status AS {nameof(CampaignResponse.Status)},
                 name AS {nameof(CampaignResponse.Name)},
                 description AS {nameof(CampaignResponse.Description)},
                 offer_id AS {nameof(CampaignResponse.OfferId)},
                 client_id AS {nameof(CampaignResponse.ClientId)},
                 creator_id AS {nameof(CampaignResponse.CreatorId)},
                 scheduled_on_utc AS {nameof(CampaignResponse.ScheduledOnUtc)},
                 pended_on_utc AS {nameof(CampaignResponse.PendedOnUtc)},
                 confirmed_on_utc AS {nameof(CampaignResponse.ConfirmedOnUtc)},
                 rejected_on_utc AS {nameof(CampaignResponse.RejectedOnUtc)},
                 cancelled_on_utc AS {nameof(CampaignResponse.CancelledOnUtc)},
                 done_on_utc AS {nameof(CampaignResponse.DoneOnUtc)},
                 completed_on_utc AS {nameof(CampaignResponse.CompletedOnUtc)}
              FROM campaigns.campaigns
              WHERE id = @CampaignId
             """;
        CampaignResponse campaign = await connection.QuerySingleOrDefaultAsync<CampaignResponse>(
            sql,
            new { request.CampaignId }
        );

        return campaign ?? Result.Failure<CampaignResponse>(CampaignErrors.NotFound);
    }
}
