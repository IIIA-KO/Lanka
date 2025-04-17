using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.MarkAsDone;

public sealed record MarkCampaignAsDoneCommand(Guid CampaignId) : ICommand;
