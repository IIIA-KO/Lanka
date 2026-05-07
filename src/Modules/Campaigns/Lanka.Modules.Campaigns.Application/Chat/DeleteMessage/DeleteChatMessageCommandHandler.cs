using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat.DeleteMessage;

internal sealed class DeleteChatMessageCommandHandler
    : ICommandHandler<DeleteChatMessageCommand>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _chatNotificationService;

    public DeleteChatMessageCommandHandler(
        IChatMessageRepository chatMessageRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IChatNotificationService chatNotificationService)
    {
        this._chatMessageRepository = chatMessageRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
        this._chatNotificationService = chatNotificationService;
    }

    public async Task<Result> Handle(DeleteChatMessageCommand request, CancellationToken cancellationToken)
    {
        ChatMessage? message = await this._chatMessageRepository.GetByIdAsync(
            new ChatMessageId(request.MessageId),
            cancellationToken);

        if (message is null || message.ThreadId.Value != request.ThreadId)
        {
            return Result.Failure(ChatMessageErrors.NotFound);
        }

        if (message.SenderBloggerId?.Value != this._userContext.GetUserId())
        {
            return Result.Failure(Error.NotAuthorized);
        }

        Result deleteResult = message.SoftDelete();
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        await this._chatNotificationService.DeleteMessageAsync(
            new ChatMessageDeletedNotification(request.ThreadId, request.MessageId),
            cancellationToken);

        return Result.Success();
    }
}
