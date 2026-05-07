using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Chat;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat.MarkRead;

internal sealed class MarkChatMessagesReadCommandHandler
    : ICommandHandler<MarkChatMessagesReadCommand>
{
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _chatNotificationService;

    public MarkChatMessagesReadCommandHandler(
        IChatThreadRepository chatThreadRepository,
        IChatMessageRepository chatMessageRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IChatNotificationService chatNotificationService)
    {
        this._chatThreadRepository = chatThreadRepository;
        this._chatMessageRepository = chatMessageRepository;
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
        this._chatNotificationService = chatNotificationService;
    }

    public async Task<Result> Handle(MarkChatMessagesReadCommand request, CancellationToken cancellationToken)
    {
        ChatThread? thread = await this._chatThreadRepository.GetByIdAsync(
            new ChatThreadId(request.ThreadId),
            cancellationToken);

        if (thread is null)
        {
            return Result.Failure(ChatThreadErrors.NotFound);
        }

        Guid readerId = this._userContext.GetUserId();
        if (!ChatAuthorization.IsParticipant(thread, readerId))
        {
            return Result.Failure(Error.NotAuthorized);
        }

        DateTimeOffset utcNow = this._dateTimeProvider.UtcNow;
        await this._chatMessageRepository.BulkMarkReadAsync(
            thread.Id,
            new BloggerId(readerId),
            utcNow,
            cancellationToken);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        await this._chatNotificationService.MarkReadAsync(
            new ChatMessagesReadNotification(request.ThreadId, readerId, utcNow.UtcDateTime),
            cancellationToken);

        return Result.Success();
    }
}
