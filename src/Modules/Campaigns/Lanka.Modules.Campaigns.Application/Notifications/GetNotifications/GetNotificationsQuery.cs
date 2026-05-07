using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Notifications.GetNotifications;

public sealed record GetNotificationsQuery : IQuery<IReadOnlyList<NotificationResponse>>;
