using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Chat.GetThreads;

public sealed record GetChatThreadsQuery : IQuery<IReadOnlyList<ChatThreadResponse>>;
