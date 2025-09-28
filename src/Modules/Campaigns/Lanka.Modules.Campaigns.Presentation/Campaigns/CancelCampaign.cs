using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.Cancel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class CancelCampaign : CampaignsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.UpdateCampaign];
    
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("{id:guid}/cancel"),
                async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(
                        new CancelCampaignCommand(id),
                        cancellationToken
                    );
                    
                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("CancelCampaign")
            .WithSummary("Cancel campaign")
            .WithDescription("Cancels an existing campaign")
            .WithOpenApi();
    }
}
