using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.BlockedDates
{
    public sealed record BlockedDateId(Guid Value) : TypedEntityId(Value)
    {
        public static BlockedDateId New() => new(Guid.NewGuid());
    }
}
