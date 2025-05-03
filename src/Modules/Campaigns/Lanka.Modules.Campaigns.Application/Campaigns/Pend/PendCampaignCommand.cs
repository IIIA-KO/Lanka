using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Pend;

public sealed record PendCampaignCommand(
    string Name,
    string Description,
    DateTimeOffset ScheduledOnUtc,
    OfferId OfferId
) : ICommand<Guid>;
