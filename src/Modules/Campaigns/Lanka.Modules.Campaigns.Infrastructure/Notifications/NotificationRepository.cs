using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Notifications;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Notifications;

internal sealed class NotificationRepository : INotificationRepository
{
    private readonly CampaignsDbContext _dbContext;

    public NotificationRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(BloggerId recipientId, CancellationToken cancellationToken)
    {
        return await this._dbContext.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken cancellationToken)
    {
        return await this._dbContext.Notifications
            .Where(n => n.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Notification notification)
    {
        this._dbContext.Notifications.Add(notification);
    }
}
