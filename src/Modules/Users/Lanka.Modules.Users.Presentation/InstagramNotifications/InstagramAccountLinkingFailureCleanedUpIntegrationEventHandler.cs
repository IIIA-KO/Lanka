using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.Application.Instagram;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingFailureCleanedUpIntegrationEvent>
{
    private readonly IInstagramOperationStatusService _statusService;

    public InstagramAccountLinkingFailureCleanedUpIntegrationEventHandler(
        IInstagramOperationStatusService statusService)
    {
        this._statusService = statusService;
    }

    public override async Task Handle(
        InstagramAccountLinkingFailureCleanedUpIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        // CorrelationId is the UserId in this context (saga correlation)
        await this._statusService.SetStatusAsync(
            integrationEvent.CorrelationId,
            InstagramOperationType.Linking,
            InstagramOperationStatuses.Failed,
            integrationEvent.Reason,
            completedAt: DateTime.UtcNow,
            cancellationToken: cancellationToken
        );
    }
}
