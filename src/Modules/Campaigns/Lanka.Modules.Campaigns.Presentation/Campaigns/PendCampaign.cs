using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Campaigns.Pend;
using Lanka.Modules.Campaigns.Domain.Offers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class PendCampaign : CampaignsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.CreateCampaign];
    
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute(string.Empty),
                async (
                    [FromBody] PendCampaignRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<Guid> result = await sender.Send(new PendCampaignCommand(
                            request.Name,
                            request.Description,
                            request.ScheduledOnUtc,
                            new OfferId(request.OfferId)
                        ),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("PendCampaign")
            .WithSummary("Create pending campaign")
            .WithDescription("Creates a new campaign in pending status")
            .WithOpenApi();
    }

    internal sealed class PendCampaignRequest
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTimeOffset ScheduledOnUtc { get; init; }
        public Guid OfferId { get; init; }
    }
}
