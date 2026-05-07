using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Chat.GetThreads;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed class GetChatThreads : ChatEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadChat];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute(string.Empty),
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    Result<IReadOnlyList<ChatThreadResponse>> result = await sender.Send(
                        new GetChatThreadsQuery(),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Campaigns)
            .WithName("GetChatThreads")
            .WithSummary("Get chat threads")
            .WithDescription("Gets the current user's chat inbox");
    }
}
