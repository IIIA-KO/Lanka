using Lanka.Common.Application.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Lanka.Common.Infrastructure.Notifications;

public sealed class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<InstagramHub> _hubContext;

    public SignalRNotificationService(IHubContext<InstagramHub> hubContext)
    {
        this._hubContext = hubContext;
    }

    public async Task SendInstagramLinkingStatusAsync(string userId, string status, string? message = null, CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            type = "instagram_linking",
            status,
            message,
            timestamp = DateTime.UtcNow
        };

        await this._hubContext.Clients.Group($"user_{userId}")
            .SendAsync("InstagramLinkingStatus", notification, cancellationToken);
    }

    public async Task SendInstagramRenewalStatusAsync(string userId, string status, string? message = null, CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            type = "instagram_renewal", 
            status,
            message,
            timestamp = DateTime.UtcNow
        };
        
        await this._hubContext.Clients.Group($"user_{userId}")
            .SendAsync("InstagramRenewalStatus", notification, cancellationToken);
    }
}

[Authorize]
public sealed class InstagramHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, $"user_{userId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}


