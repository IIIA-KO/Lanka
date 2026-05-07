using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Notifications;

namespace Lanka.Modules.Campaigns.Application.Notifications.MarkAllNotificationsRead;

internal sealed class MarkAllNotificationsReadCommandHandler : ICommandHandler<MarkAllNotificationsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._notificationRepository = notificationRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var recipientId = new BloggerId(this._userContext.GetUserId());

        IReadOnlyList<Notification> notifications =
            await this._notificationRepository.GetByRecipientAsync(recipientId, cancellationToken);

        foreach (Notification notification in notifications.Where(n => !n.IsRead))
        {
            notification.MarkAsRead();
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
