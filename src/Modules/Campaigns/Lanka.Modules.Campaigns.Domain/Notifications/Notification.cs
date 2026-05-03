using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.Notifications;

public class Notification : Entity<NotificationId>
{
    public BloggerId RecipientId { get; private set; }

    public Guid CampaignId { get; private set; }

    public string Title { get; private set; }

    public string Body { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Notification() { }

    public static Notification Create(
        BloggerId recipientId,
        Guid campaignId,
        string title,
        string body,
        DateTimeOffset utcNow
    )
    {
        return new Notification
        {
            Id = NotificationId.New(),
            RecipientId = recipientId,
            CampaignId = campaignId,
            Title = title,
            Body = body,
            IsRead = false,
            CreatedAtUtc = utcNow
        };
    }

    public void MarkAsRead()
    {
        this.IsRead = true;
    }
}
