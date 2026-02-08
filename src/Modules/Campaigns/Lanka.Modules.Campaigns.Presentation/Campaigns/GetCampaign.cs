using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class GetCampaign : CampaignsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadCampaigns];
    
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("{id:guid}"),
                async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<CampaignResponse> result = await sender.Send(
                        new GetCampaignQuery(id),
                        cancellationToken
                    );
                    
                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("GetCampaign")
            .WithSummary("Get campaign")
            .WithDescription("Retrieves campaign details by ID");
    }
}
