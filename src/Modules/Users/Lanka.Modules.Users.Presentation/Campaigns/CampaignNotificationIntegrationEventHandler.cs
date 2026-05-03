using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using Lanka.Modules.Users.Application.Users.GetUser;
using MediatR;

namespace Lanka.Modules.Users.Presentation.Campaigns;

internal sealed class CampaignNotificationIntegrationEventHandler
    : IntegrationEventHandler<CampaignNotificationIntegrationEvent>
{
    private readonly ISender _sender;
    private readonly INotificationService _notificationService;

    public CampaignNotificationIntegrationEventHandler(ISender sender, INotificationService notificationService)
    {
        this._sender = sender;
        this._notificationService = notificationService;
    }

    public override async Task Handle(
        CampaignNotificationIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UserResponse> result = await this._sender.Send(
            new GetUserQuery(integrationEvent.RecipientUserId),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetUserQuery), result.Error);
        }

        await this._notificationService.SendCampaignNotificationAsync(
            result.Value.IdentityId,
            integrationEvent.CampaignId,
            integrationEvent.CampaignName,
            integrationEvent.NewStatus,
            cancellationToken
        );
    }
}
