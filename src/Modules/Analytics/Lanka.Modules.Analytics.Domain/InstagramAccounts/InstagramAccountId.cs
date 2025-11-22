using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts;

public sealed record InstagramAccountId(Guid Value) : TypedEntityId(Value)
{
    public static InstagramAccountId New() => new(Guid.CreateVersion7());
}
