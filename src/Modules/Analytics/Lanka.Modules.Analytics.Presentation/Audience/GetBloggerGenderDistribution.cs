using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Audience;

internal sealed class GetBloggerGenderDistribution : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("audience/{userId:guid}/gender-distribution"),
                async (
                    [FromRoute] Guid userId,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<GenderDistributionResponse> result = await sender.Send(
                        new GetGenderDistributionQuery(userId),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerGenderDistribution")
            .WithSummary("Get blogger audience gender distribution")
            .WithDescription("Retrieves gender distribution data for a specific blogger's Instagram account audience");
    }
}
