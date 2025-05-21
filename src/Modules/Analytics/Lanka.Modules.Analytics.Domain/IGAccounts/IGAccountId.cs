using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.IGAccounts;

public sealed record IGAccountId(Guid Value) : TypedEntityId(Value)
{
    public static IGAccountId New() => new(Guid.NewGuid());
}
