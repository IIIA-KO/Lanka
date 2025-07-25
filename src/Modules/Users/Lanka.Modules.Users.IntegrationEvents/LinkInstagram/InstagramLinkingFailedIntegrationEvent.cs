using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

public sealed class InstagramLinkingFailedIntegrationEvent : IntegrationEvent
{
    public InstagramLinkingFailedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string reason
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Reason = reason;
    }

    public Guid UserId { get; }
    public string Reason { get; set; }
}
