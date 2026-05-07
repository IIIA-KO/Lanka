using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Chat.DeleteMessage;

public sealed record DeleteChatMessageCommand(Guid ThreadId, Guid MessageId) : ICommand;
