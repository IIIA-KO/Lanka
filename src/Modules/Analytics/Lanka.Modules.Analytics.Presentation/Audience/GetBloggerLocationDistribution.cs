using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;
using Lanka.Modules.Analytics.Domain.Audience;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Audience;

internal sealed class GetBloggerLocationDistribution : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("audience/{userId:guid}/location-distribution"),
                async (
                    [FromRoute] Guid userId,
                    [FromQuery] LocationType locationType,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<LocationDistributionResponse> result = await sender.Send(
                        new GetLocationDistributionQuery(userId, locationType),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerLocationDistribution")
            .WithSummary("Get blogger audience location distribution")
            .WithDescription("Retrieves location distribution data for a specific blogger's Instagram account audience");
    }
}
