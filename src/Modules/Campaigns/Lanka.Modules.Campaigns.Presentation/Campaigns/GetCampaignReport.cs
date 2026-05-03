using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaignReport;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class GetCampaignReport : CampaignsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadCampaigns];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("{id:guid}/report"),
                async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<CampaignReportResponse> result = await sender.Send(
                        new GetCampaignReportQuery(id),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("GetCampaignReport")
            .WithSummary("Get campaign report")
            .WithDescription("Retrieves the work report submitted by the creator when marking a campaign as done");
    }
}
