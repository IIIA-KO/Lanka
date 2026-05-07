using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;

namespace Lanka.Modules.Campaigns.Application.Chat.EditMessage;

public sealed record EditChatMessageCommand(Guid ThreadId, Guid MessageId, string NewContent) : ICommand<ChatMessageResponse>;
