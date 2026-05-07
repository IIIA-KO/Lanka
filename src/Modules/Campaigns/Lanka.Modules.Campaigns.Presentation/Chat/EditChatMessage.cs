using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.EditMessage;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class EditChatMessage : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.WriteChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPatch(this.BuildRoute("{threadId:guid}/messages/{messageId:guid}"),
                async (
                    [FromRoute] Guid threadId,
                    [FromRoute] Guid messageId,
                    [FromBody] EditChatMessageRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<ChatMessageResponse> result = await sender.Send(
                        new EditChatMessageCommand(threadId, messageId, request.NewContent),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("EditChatMessage")
            .WithSummary("Edit chat message")
            .WithDescription("Edits a chat message sent by the caller");
    }
}
