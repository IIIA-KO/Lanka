using Lanka.Common.Application.Caching;
using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Presentation.Analytics;

internal sealed class InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingFailureCleanedUpIntegrationEvent>
{
    private readonly ILogger<InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler(
        ILogger<InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler> logger,
        ICacheService cacheService
    )
    {
        this._logger = logger;
        this._cacheService = cacheService;
    }

    public override async Task Handle(
        InstagramAccountLinkingFailureCleanedUpIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogWarning(
            "Instagram account linking failure cleaned up for CorrelationId {CorrelationId} with error: {Error}",
            integrationEvent.CorrelationId, integrationEvent.Reason
        );
        string cacheKey = $"ig:link:{integrationEvent.CorrelationId}";
        await this._cacheService.SetAsync(cacheKey, "failed", TimeSpan.FromMinutes(10), cancellationToken);
    }
}
