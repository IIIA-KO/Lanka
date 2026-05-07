using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Notifications;

public static class NotificationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Notifications.NotFound",
        "The notification was not found"
    );
}
