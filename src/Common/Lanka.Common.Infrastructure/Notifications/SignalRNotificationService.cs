using Lanka.Common.Application.Notifications;
using Lanka.Common.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Lanka.Common.Infrastructure.Notifications;

public sealed class SignalRNotificationService : INotificationService, IChatNotificationService
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

    public async Task SendCampaignNotificationAsync(string identityId, Guid campaignId, string campaignName, string newStatus, CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            campaignId,
            campaignName,
            newStatus,
            timestamp = DateTime.UtcNow
        };

        await this._hubContext.Clients.Group($"user_{identityId}")
            .SendAsync("CampaignNotification", notification, cancellationToken);
    }

    public async Task SendMessageAsync(ChatMessageNotification message, CancellationToken cancellationToken = default)
    {
        await this._hubContext.Clients.Group(GetChatGroupName(message.ThreadId))
            .SendAsync("ChatMessageSent", message, cancellationToken);

        if (message.RecipientBloggerId.HasValue)
        {
            await this._hubContext.Clients.Group(GetUserGroupName(message.RecipientBloggerId.Value))
                .SendAsync("ChatMessageSent", message, cancellationToken);
        }
    }

    public async Task EditMessageAsync(ChatMessageNotification message, CancellationToken cancellationToken = default)
    {
        await this._hubContext.Clients.Group(GetChatGroupName(message.ThreadId))
            .SendAsync("ChatMessageEdited", message, cancellationToken);
    }

    public async Task DeleteMessageAsync(ChatMessageDeletedNotification notification, CancellationToken cancellationToken = default)
    {
        await this._hubContext.Clients.Group(GetChatGroupName(notification.ThreadId))
            .SendAsync("ChatMessageDeleted", notification, cancellationToken);
    }

    public async Task MarkReadAsync(ChatMessagesReadNotification notification, CancellationToken cancellationToken = default)
    {
        await this._hubContext.Clients.Group(GetChatGroupName(notification.ThreadId))
            .SendAsync("ChatMessagesRead", notification, cancellationToken);
    }

    private static string GetChatGroupName(Guid threadId) => $"chat_{threadId}";

    private static string GetUserGroupName(Guid userId) => $"user_{userId}";
}

[Authorize]
public sealed class InstagramHub : Hub
{
    private readonly IChatMembershipService _chatMembershipService;

    public InstagramHub(IChatMembershipService chatMembershipService)
    {
        this._chatMembershipService = chatMembershipService;
    }

    public override async Task OnConnectedAsync()
    {
        string? userId = this.Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userId = this.Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(Guid threadId)
    {
        Guid userId = this.Context.User.GetUserId();
        bool canJoin = await this._chatMembershipService.CanJoinChatAsync(
            threadId,
            userId,
            this.Context.ConnectionAborted);

        if (!canJoin)
        {
            throw new HubException("You are not a participant in this chat.");
        }

        await this.Groups.AddToGroupAsync(
            this.Context.ConnectionId,
            $"chat_{threadId}",
            this.Context.ConnectionAborted);
    }

    public async Task LeaveChat(Guid threadId)
    {
        await this.Groups.RemoveFromGroupAsync(
            this.Context.ConnectionId,
            $"chat_{threadId}",
            this.Context.ConnectionAborted);
    }
}
