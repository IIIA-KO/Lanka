using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.GetThreads;
using Lanka.Modules.Campaigns.Application.Chat.StartThread;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class StartChatThread : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.WriteChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("start"),
                async (
                    [FromBody] StartChatThreadRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<ChatThreadResponse> result = await sender.Send(
                        new StartChatThreadCommand(
                            request.ParticipantBloggerId,
                            request.OfferId,
                            request.CampaignId),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("StartChatThread")
            .WithSummary("Start chat thread")
            .WithDescription("Starts or reuses a chat thread with another blogger");
    }
}
