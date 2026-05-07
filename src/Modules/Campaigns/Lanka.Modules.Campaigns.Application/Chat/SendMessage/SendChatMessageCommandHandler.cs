using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Chat;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat.SendMessage;

internal sealed class SendChatMessageCommandHandler
    : ICommandHandler<SendChatMessageCommand, ChatMessageResponse>
{
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _chatNotificationService;

    public SendChatMessageCommandHandler(
        IChatThreadRepository chatThreadRepository,
        IBloggerRepository bloggerRepository,
        IChatMessageRepository chatMessageRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IChatNotificationService chatNotificationService)
    {
        this._chatThreadRepository = chatThreadRepository;
        this._bloggerRepository = bloggerRepository;
        this._chatMessageRepository = chatMessageRepository;
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
        this._chatNotificationService = chatNotificationService;
    }

    public async Task<Result<ChatMessageResponse>> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        ChatThread? thread = await this._chatThreadRepository.GetByIdAsync(
            new ChatThreadId(request.ThreadId),
            cancellationToken);

        if (thread is null)
        {
            return Result.Failure<ChatMessageResponse>(ChatThreadErrors.NotFound);
        }

        Guid senderId = this._userContext.GetUserId();
        if (!ChatAuthorization.IsParticipant(thread, senderId))
        {
            return Result.Failure<ChatMessageResponse>(Error.NotAuthorized);
        }

        Blogger? sender = await this._bloggerRepository.GetByIdAsync(new BloggerId(senderId), cancellationToken);
        if (sender is null)
        {
            return Result.Failure<ChatMessageResponse>(Error.NotAuthorized);
        }

        Result<ChatMessage> messageResult = ChatMessage.CreateUserMessage(
            thread.Id,
            sender.Id,
            request.Content,
            this._dateTimeProvider.UtcNow);

        if (messageResult.IsFailure)
        {
            return Result.Failure<ChatMessageResponse>(messageResult.Error);
        }

        ChatMessage message = messageResult.Value;
        this._chatMessageRepository.Add(message);
        thread.Touch(message.CreatedAtUtc);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ChatMessageResponse(
            message.Id.Value,
            message.ThreadId.Value,
            message.SenderBloggerId?.Value,
            sender.FirstName.Value,
            sender.LastName.Value,
            message.Content,
            message.IsSystem,
            message.IsDeleted,
            message.EditedAtUtc?.UtcDateTime,
            message.ReadAtUtc?.UtcDateTime,
            message.CreatedAtUtc.UtcDateTime);

        await this._chatNotificationService.SendMessageAsync(
            ToNotification(response),
            cancellationToken);

        return response;
    }

    private static ChatMessageNotification ToNotification(ChatMessageResponse response)
    {
        return new ChatMessageNotification(
            response.Id,
            response.ThreadId,
            response.SenderBloggerId,
            response.SenderFirstName,
            response.SenderLastName,
            response.Content,
            response.IsSystem,
            response.IsDeleted,
            response.EditedAtUtc,
            response.ReadAtUtc,
            response.CreatedAtUtc);
    }
}
