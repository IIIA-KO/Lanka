using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Offers;

public sealed record OfferId(Guid Value) : TypedEntityId(Value)
{
    public static OfferId New() => new(Guid.NewGuid());
}
