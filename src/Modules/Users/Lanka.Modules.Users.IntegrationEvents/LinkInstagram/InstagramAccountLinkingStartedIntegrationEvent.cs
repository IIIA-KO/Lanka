using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

public sealed class InstagramAccountLinkingStartedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountLinkingStartedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string email,
        string code
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Email = email;
        this.Code = code;
    }

    public Guid UserId { get; }

    public string Email { get; }

    public string Code { get; }
}
