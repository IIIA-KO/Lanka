using Lanka.Common.Application.EventBus;
using Lanka.Modules.Users.Application.Instagram;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

namespace Lanka.Modules.Users.Presentation.InstagramNotifications;

internal sealed class InstagramAccountLinkingCompletedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountLinkingCompletedIntegrationEvent>
{
    private readonly IInstagramOperationStatusService _statusService;

    public InstagramAccountLinkingCompletedIntegrationEventHandler(
        IInstagramOperationStatusService statusService)
    {
        this._statusService = statusService;
    }

    public override async Task Handle(
        InstagramAccountLinkingCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        await this._statusService.SetStatusAsync(
            integrationEvent.UserId,
            InstagramOperationType.Linking,
            InstagramOperationStatuses.Completed,
            "Instagram account linked successfully",
            completedAt: DateTime.UtcNow,
            cancellationToken: cancellationToken
        );
    }
}
