using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.GetBloggerCampaigns;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class GetBloggerCampaigns : CampaignsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadCampaigns];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("bloggers/{bloggerId:guid}"),
                async (
                    [FromRoute] Guid bloggerId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<IReadOnlyList<CampaignResponse>> result = await sender.Send(
                        new GetBloggerCampaignsQuery(bloggerId),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("GetBloggerCampaigns")
            .WithSummary("Get blogger campaigns")
            .WithDescription("Retrieves all campaigns for a specific blogger (as client or creator)");
    }
}
