using Lanka.Common.Domain;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Campaigns.Application.Campaigns.SeedCampaigns;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lanka.Modules.Campaigns.Presentation.Campaigns;

internal sealed class SeedCampaigns : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        IHostEnvironment environment = app.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (!environment.IsDevelopment())
        {
            return;
        }

        app.MapPost("dev/campaigns/seed",
                async (
                    [FromQuery] Guid bloggerId,
                    [FromQuery] int campaignsPerMonth,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<SeedCampaignsResult> result = await sender.Send(
                        new SeedCampaignsCommand(bloggerId, campaignsPerMonth),
                        cancellationToken
                    );

                    return result;
                })
            .AllowAnonymous()
            .WithTags("Dev")
            .WithName("SeedCampaigns")
            .WithSummary("Seed campaigns using existing bloggers/pacts/offers (development only)");
    }
}
