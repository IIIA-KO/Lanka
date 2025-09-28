using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Offers.DomainEvents;

public sealed class OfferUpdatedDomainEvent(OfferId offerId) : DomainEvent
{
    public OfferId OfferId { get; init; } = offerId;
}

