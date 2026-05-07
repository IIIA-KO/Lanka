using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Payments;

public sealed record PaymentId(Guid Value) : TypedEntityId(Value)
{
    public static PaymentId New() => new(Guid.CreateVersion7());
}
