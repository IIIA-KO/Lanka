using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Notifications.MarkAllNotificationsRead;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Notifications;

internal sealed class MarkAllNotificationsRead : NotificationsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.UpdateNotifications];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("read-all"),
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken);

                    return result.Match(Results.NoContent, ApiResult.Problem);
                })
            .WithTags(Tags.Notifications)
            .WithName("MarkAllNotificationsRead")
            .WithSummary("Mark all notifications as read")
            .WithDescription("Marks all notifications for the current user as read");
    }
}
