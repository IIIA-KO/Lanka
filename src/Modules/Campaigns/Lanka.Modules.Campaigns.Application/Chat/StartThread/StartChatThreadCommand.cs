using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Chat.GetThreads;

namespace Lanka.Modules.Campaigns.Application.Chat.StartThread;

public sealed record StartChatThreadCommand(
    Guid ParticipantBloggerId,
    Guid? OfferId = null,
    Guid? CampaignId = null) : ICommand<ChatThreadResponse>;
