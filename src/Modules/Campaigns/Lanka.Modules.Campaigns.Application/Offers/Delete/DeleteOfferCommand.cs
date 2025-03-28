using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Offers.Delete;

public sealed record DeleteOfferCommand(Guid OfferId) : ICommand;
