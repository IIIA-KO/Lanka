using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;

namespace Lanka.Modules.Campaigns.Application.Chat.SendMessage;

public sealed record SendChatMessageCommand(Guid ThreadId, string Content) : ICommand<ChatMessageResponse>;
