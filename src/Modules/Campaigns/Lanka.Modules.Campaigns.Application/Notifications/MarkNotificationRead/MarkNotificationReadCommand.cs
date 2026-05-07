using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand;
