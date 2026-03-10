using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;

namespace Lanka.Modules.Campaigns.Application.Campaigns.GetBloggerCampaigns;

internal sealed class GetBloggerCampaignsQueryHandler
    : IQueryHandler<GetBloggerCampaignsQuery, IReadOnlyList<CampaignResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetBloggerCampaignsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<CampaignResponse>>> Handle(
        GetBloggerCampaignsQuery request,
        CancellationToken cancellationToken)
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
             WHERE client_id = @BloggerId OR creator_id = @BloggerId
             ORDER BY scheduled_on_utc DESC
             """;

        IEnumerable<CampaignResponse> campaigns = await connection.QueryAsync<CampaignResponse>(
            sql,
            new { request.BloggerId }
        );

        return campaigns.ToList();
    }
}
