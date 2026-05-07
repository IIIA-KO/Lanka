using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public sealed record ChatThreadId(Guid Value) : TypedEntityId(Value)
{
    public static ChatThreadId New() => new(Guid.CreateVersion7());
}
