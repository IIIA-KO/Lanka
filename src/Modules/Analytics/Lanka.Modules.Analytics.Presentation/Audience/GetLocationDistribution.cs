using Lanka.Common.Application.Authentication;
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

internal sealed class GetLocationDistribution : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("audience/location-distribution"),
                async (
                    [FromQuery] LocationType locationType,
                    ISender sender,
                    IUserContext userContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<LocationDistributionResponse> result = await sender.Send(
                        new GetLocationDistributionQuery(
                            userContext.GetUserId(),
                            locationType
                        ),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetLocationDistribution")
            .WithSummary("Get audience location distribution")
            .WithDescription("Retrieves location distribution data for Instagram account audience by location type");
    }
}
