using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Notifications;

namespace Lanka.Modules.Campaigns.Application.Notifications.GetNotifications;

internal sealed class GetNotificationsQueryHandler
    : IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository, IUserContext userContext)
    {
        this._notificationRepository = notificationRepository;
        this._userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<NotificationResponse>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var recipientId = new BloggerId(this._userContext.GetUserId());

        IReadOnlyList<Notification> notifications =
            await this._notificationRepository.GetByRecipientAsync(recipientId, cancellationToken);

        return notifications
            .Select(n => new NotificationResponse(
                n.Id.Value,
                n.CampaignId,
                n.Title,
                n.Body,
                n.IsRead,
                n.CreatedAtUtc
            ))
            .ToList();
    }
}
