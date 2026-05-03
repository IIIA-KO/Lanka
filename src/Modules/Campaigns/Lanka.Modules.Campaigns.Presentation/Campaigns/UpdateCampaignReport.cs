using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.UpdateReport;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class UpdateCampaignReport : CampaignsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.UpdateCampaign];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("{id:guid}/report"),
                async (
                    [FromRoute] Guid id,
                    [FromBody] UpdateCampaignReportRequest req,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(
                        new UpdateCampaignReportCommand(
                            id,
                            req.ContentDelivered,
                            req.Approach,
                            req.Notes,
                            req.PostPermalinks ?? []
                        ),
                        cancellationToken
                    );

                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("UpdateCampaignReport")
            .WithSummary("Update campaign report")
            .WithDescription("Updates the work report for a Done campaign (creator only, before client completes)");
    }
}

internal sealed record UpdateCampaignReportRequest(
    string ContentDelivered,
    string Approach,
    string? Notes,
    List<string>? PostPermalinks
);
