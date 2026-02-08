using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.Application.Instagram;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramRenewalAccessFailureCleanedUpIntegrationEventHandler
    : IntegrationEventHandler<InstagramRenewalAccessFailureCleanedUpIntegrationEvent>
{
    private readonly IInstagramOperationStatusService _statusService;

    public InstagramRenewalAccessFailureCleanedUpIntegrationEventHandler(
        IInstagramOperationStatusService statusService)
    {
        this._statusService = statusService;
    }

    public override async Task Handle(
        InstagramRenewalAccessFailureCleanedUpIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        // CorrelationId is the UserId in this context (saga correlation)
        await this._statusService.SetStatusAsync(
            integrationEvent.CorrelationId,
            InstagramOperationType.Renewal,
            InstagramOperationStatuses.Failed,
            integrationEvent.Reason,
            completedAt: DateTime.UtcNow,
            cancellationToken: cancellationToken
        );
    }
}
