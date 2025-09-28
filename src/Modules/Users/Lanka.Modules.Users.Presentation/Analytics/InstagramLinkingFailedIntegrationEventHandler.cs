using Lanka.Common.Application.Caching;
using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Presentation.Analytics;

internal sealed class InstagramLinkingFailedIntegrationEventHandler
    : IntegrationEventHandler<InstagramLinkingFailedIntegrationEvent>
{
    private readonly ILogger<InstagramLinkingFailedIntegrationEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public InstagramLinkingFailedIntegrationEventHandler(
        ILogger<InstagramLinkingFailedIntegrationEventHandler> logger,
        ICacheService cacheService)
    {
        this._logger = logger;
        this._cacheService = cacheService;
    }

    public override async Task Handle(
        InstagramLinkingFailedIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogWarning("Instagram linking cleaned up for user {UserId} with error: {Error}",
            integrationEvent.UserId, integrationEvent.Reason
        );
        string cacheKey = $"ig:link:{integrationEvent.UserId}";
        await this._cacheService.SetAsync(cacheKey, "failed", TimeSpan.FromMinutes(10), cancellationToken);
    }
}
