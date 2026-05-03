using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Notifications;

public sealed record NotificationId(Guid Value) : TypedEntityId(Value)
{
    public static NotificationId New() => new(Guid.CreateVersion7());
}
