using Lanka.Common.Presentation.Endpoints;

namespace Lanka.Modules.Campaigns.Presentation.Notifications;

internal abstract class NotificationsEndpointBase : EndpointBase
{
    protected override string BaseRoute => "notifications";
}
