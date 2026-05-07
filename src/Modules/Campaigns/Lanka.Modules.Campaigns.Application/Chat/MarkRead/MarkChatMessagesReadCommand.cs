using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Chat.MarkRead;

public sealed record MarkChatMessagesReadCommand(Guid ThreadId) : ICommand;
