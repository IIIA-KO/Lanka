using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Audience;

internal sealed class GetBloggerAgeDistribution : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("audience/{userId:guid}/age-distribution"),
                async (
                    [FromRoute] Guid userId,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<AgeDistributionResponse> result = await sender.Send(
                        new GetAgeDistributionQuery(userId),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerAgeDistribution")
            .WithSummary("Get blogger audience age distribution")
            .WithDescription("Retrieves age distribution data for a specific blogger's Instagram account audience");
    }
}
