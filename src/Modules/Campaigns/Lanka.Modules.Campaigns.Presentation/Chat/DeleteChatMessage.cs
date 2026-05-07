using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.DeleteMessage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class DeleteChatMessage : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.WriteChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapDelete(this.BuildRoute("{threadId:guid}/messages/{messageId:guid}"),
                async (
                    [FromRoute] Guid threadId,
                    [FromRoute] Guid messageId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(
                        new DeleteChatMessageCommand(threadId, messageId),
                        cancellationToken);

                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("DeleteChatMessage")
            .WithSummary("Delete chat message")
            .WithDescription("Soft-deletes a chat message sent by the caller");
    }
}
