using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Chat.GetMessages;

public sealed record GetChatMessagesQuery(
    Guid ThreadId,
    DateTimeOffset? Before,
    int Limit = 30) : IQuery<IReadOnlyList<ChatMessageResponse>>;
