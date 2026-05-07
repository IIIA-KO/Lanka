using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Notifications;
using Lanka.Modules.Campaigns.Application.Notifications.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Notifications;

internal sealed class GetNotifications : NotificationsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadNotifications];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute(string.Empty),
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    Result<IReadOnlyList<NotificationResponse>> result =
                        await sender.Send(new GetNotificationsQuery(), cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Notifications)
            .WithName("GetNotifications")
            .WithSummary("Get notifications")
            .WithDescription("Returns all notifications for the current user");
    }
}
