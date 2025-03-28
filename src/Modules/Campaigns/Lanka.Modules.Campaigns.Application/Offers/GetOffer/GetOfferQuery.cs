using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Offers.GetOffer;

public sealed record GetOfferQuery(Guid OfferId) : IQuery<OfferResponse>;
