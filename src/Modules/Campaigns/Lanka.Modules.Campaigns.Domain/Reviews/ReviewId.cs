using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews;

public sealed record ReviewId(Guid Value) : TypedEntityId(Value)
{
    public static ReviewId New() => new(Guid.NewGuid());
}
