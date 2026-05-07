using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;
using Lanka.Modules.Campaigns.Application.Chat.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class SendChatMessage : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.WriteChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("{threadId:guid}/messages"),
                async (
                    [FromRoute] Guid threadId,
                    [FromBody] SendChatMessageRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<ChatMessageResponse> result = await sender.Send(
                        new SendChatMessageCommand(threadId, request.Content),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("SendChatMessage")
            .WithSummary("Send chat message")
            .WithDescription("Sends a new chat message");
    }
}
