using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public sealed record ChatMessageId(Guid Value) : TypedEntityId(Value)
{
    public static ChatMessageId New() => new(Guid.CreateVersion7());
}
