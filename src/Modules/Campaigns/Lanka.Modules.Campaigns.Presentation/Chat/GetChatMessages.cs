using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class GetChatMessages : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("{threadId:guid}/messages"),
                async (
                    [FromRoute] Guid threadId,
                    [FromQuery] DateTimeOffset? before,
                    [FromQuery] int? limit,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<IReadOnlyList<ChatMessageResponse>> result = await sender.Send(
                        new GetChatMessagesQuery(threadId, before, limit ?? 30),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("GetChatMessages")
            .WithSummary("Get chat messages")
            .WithDescription("Gets paginated chat messages");
    }
}
