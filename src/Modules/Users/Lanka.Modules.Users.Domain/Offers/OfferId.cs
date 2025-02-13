using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Offers
{
    public sealed record OfferId(Guid Value) : TypedEntityId(Value)
    {
        public static OfferId New() => new(Guid.NewGuid());
    }
}
