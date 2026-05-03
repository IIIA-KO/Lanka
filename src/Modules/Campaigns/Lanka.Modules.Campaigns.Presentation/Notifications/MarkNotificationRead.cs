using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Notifications.MarkNotificationRead;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Notifications;

internal sealed class MarkNotificationRead : NotificationsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.UpdateNotifications];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("{id:guid}/read"),
                async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(new MarkNotificationReadCommand(id), cancellationToken);

                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Notifications)
            .WithName("MarkNotificationRead")
            .WithSummary("Mark notification as read")
            .WithDescription("Marks a single notification as read");
    }
}
