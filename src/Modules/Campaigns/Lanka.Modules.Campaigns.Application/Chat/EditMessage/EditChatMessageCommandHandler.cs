using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Chat.GetMessages;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat.EditMessage;

internal sealed class EditChatMessageCommandHandler
    : ICommandHandler<EditChatMessageCommand, ChatMessageResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _chatNotificationService;

    public EditChatMessageCommandHandler(
        IChatMessageRepository chatMessageRepository,
        IBloggerRepository bloggerRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IChatNotificationService chatNotificationService)
    {
        this._chatMessageRepository = chatMessageRepository;
        this._bloggerRepository = bloggerRepository;
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
        this._chatNotificationService = chatNotificationService;
    }

    public async Task<Result<ChatMessageResponse>> Handle(
        EditChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        ChatMessage? message = await this._chatMessageRepository.GetByIdAsync(
            new ChatMessageId(request.MessageId),
            cancellationToken);

        if (message is null || message.ThreadId.Value != request.ThreadId)
        {
            return Result.Failure<ChatMessageResponse>(ChatMessageErrors.NotFound);
        }

        Guid userId = this._userContext.GetUserId();
        if (message.SenderBloggerId?.Value != userId)
        {
            return Result.Failure<ChatMessageResponse>(Error.NotAuthorized);
        }

        Result editResult = message.Edit(request.NewContent, this._dateTimeProvider.UtcNow);
        if (editResult.IsFailure)
        {
            return Result.Failure<ChatMessageResponse>(editResult.Error);
        }

        Blogger? sender = await this._bloggerRepository.GetByIdAsync(new BloggerId(userId), cancellationToken);
        if (sender is null)
        {
            return Result.Failure<ChatMessageResponse>(Error.NotAuthorized);
        }

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

        await this._chatNotificationService.EditMessageAsync(
            new ChatMessageNotification(
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
                response.CreatedAtUtc),
            cancellationToken);

        return response;
    }
}
