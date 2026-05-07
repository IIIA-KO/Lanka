using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.MarkRead;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class MarkChatMessagesRead : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.WriteChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("{threadId:guid}/messages/read"),
                async (
                    [FromRoute] Guid threadId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(
                        new MarkChatMessagesReadCommand(threadId),
                        cancellationToken);

                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("MarkChatMessagesRead")
            .WithSummary("Mark chat messages as read")
            .WithDescription("Marks unread chat messages from the other participant as read");
    }
}
