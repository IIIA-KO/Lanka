using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Presentation.Analytics;

internal sealed class InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingFailureCleanedUpIntegrationEvent>
{
    private readonly ILogger<InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler> _logger;

    public InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler(
        ILogger<InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler> logger
    )
    {
        this._logger = logger;
    }

    public override Task Handle(
        InstagramAccountLinkingFailureCleanedUpIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogWarning(
            "Instagram account linking failure cleaned up for CorrelationId {CorrelationId} with error: {Error}",
            integrationEvent.CorrelationId, integrationEvent.Reason
        );
        
        return Task.CompletedTask;
    }
}
