using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.Complete;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class CompleteCampaign : CampaignsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("{id:guid}/complete"),
                async (Guid id, ISender sender) =>
                {
                    Result result = await sender.Send(new CompleteCampaignCommand(new CampaignId(id)));
                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns);
    }
}
