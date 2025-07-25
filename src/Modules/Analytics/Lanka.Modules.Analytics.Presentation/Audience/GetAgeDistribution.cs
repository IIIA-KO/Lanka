using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Audience;

internal sealed class GetAgeDistribution : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("audience/age-distribution"),
                async (
                    ISender sender,
                    IUserContext userContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<AgeDistribution> result = await sender.Send(
                        new GetAgeDistributionQuery(
                            userContext.GetUserId()
                        ),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics);
    }
}
