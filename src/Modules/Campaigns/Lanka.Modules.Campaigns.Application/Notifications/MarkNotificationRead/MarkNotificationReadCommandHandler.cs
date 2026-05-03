using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Notifications;

namespace Lanka.Modules.Campaigns.Application.Notifications.MarkNotificationRead;

internal sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._notificationRepository = notificationRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notificationId = new NotificationId(request.NotificationId);

        Notification? notification = await this._notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification is null)
        {
            return Result.Failure(NotificationErrors.NotFound);
        }

        var recipientId = new BloggerId(this._userContext.GetUserId());

        if (notification.RecipientId != recipientId)
        {
            return Result.Failure(Error.NotAuthorized);
        }

        notification.MarkAsRead();

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
