using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

public sealed record TokenId(Guid Value) : TypedEntityId(Value)
{
    public static TokenId New() => new(Guid.NewGuid());
}
