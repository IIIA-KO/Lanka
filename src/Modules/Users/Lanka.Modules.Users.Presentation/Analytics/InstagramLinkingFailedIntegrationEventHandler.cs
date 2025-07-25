using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Presentation.Analytics;

internal sealed class InstagramLinkingFailedIntegrationEventHandler
    : IntegrationEventHandler<InstagramLinkingFailedIntegrationEvent>
{
    private readonly ILogger<InstagramLinkingFailedIntegrationEventHandler> _logger;

    public InstagramLinkingFailedIntegrationEventHandler(ILogger<InstagramLinkingFailedIntegrationEventHandler> logger)
    {
        this._logger = logger;
    }

    public override Task Handle(
        InstagramLinkingFailedIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogWarning("Instagram linking cleaned up for user {UserId} with error: {Error}",
            integrationEvent.UserId, integrationEvent.Reason
        );
        
        return Task.CompletedTask;
    }
}
