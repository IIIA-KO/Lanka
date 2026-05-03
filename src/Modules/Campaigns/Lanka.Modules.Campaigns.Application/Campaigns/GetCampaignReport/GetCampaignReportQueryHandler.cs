using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaignReport;

internal sealed class GetCampaignReportQueryHandler
    : IQueryHandler<GetCampaignReportQuery, CampaignReportResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetCampaignReportQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<CampaignReportResponse>> Handle(
        GetCampaignReportQuery request,
        CancellationToken cancellationToken
    )
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 report_content_delivered AS {nameof(ReportRow.ContentDelivered)},
                 report_approach AS {nameof(ReportRow.Approach)},
                 report_notes AS {nameof(ReportRow.Notes)},
                 NULLIF(array_to_string(report_post_permalinks, ','), '') AS {nameof(ReportRow.PostPermalinks)},
                 report_submitted_on_utc AS {nameof(ReportRow.SubmittedOnUtc)}
             FROM campaigns.campaigns
             WHERE id = @CampaignId
               AND report_content_delivered IS NOT NULL
             """;

        ReportRow? row = await connection.QuerySingleOrDefaultAsync<ReportRow>(
            sql,
            new { request.CampaignId }
        );

        if (row is null)
        {
            return Result.Failure<CampaignReportResponse>(CampaignErrors.NotFound);
        }

        List<string> permalinks = row.PostPermalinks
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToList() ?? [];

        return new CampaignReportResponse(
            row.ContentDelivered,
            row.Approach,
            row.Notes,
            permalinks,
            row.SubmittedOnUtc
        );
    }

    private sealed class ReportRow
    {
        public string ContentDelivered { get; set; } = string.Empty;
        public string Approach { get; set; } = string.Empty;
#pragma warning disable S3459
        public string? Notes { get; set; }
        public string? PostPermalinks { get; set; }
        public DateTimeOffset SubmittedOnUtc { get; set; }
#pragma warning restore S3459
    }
}
