using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.Application.Instagram;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramAccessRenewalCompletedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccessRenewalCompletedIntegrationEvent>
{
    private readonly IInstagramOperationStatusService _statusService;

    public InstagramAccessRenewalCompletedIntegrationEventHandler(
        IInstagramOperationStatusService statusService)
    {
        this._statusService = statusService;
    }

    public override async Task Handle(
        InstagramAccessRenewalCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        await this._statusService.SetStatusAsync(
            integrationEvent.UserId,
            InstagramOperationType.Renewal,
            InstagramOperationStatuses.Completed,
            "Instagram access renewed successfully",
            completedAt: DateTime.UtcNow,
            cancellationToken: cancellationToken
        );
    }
}
