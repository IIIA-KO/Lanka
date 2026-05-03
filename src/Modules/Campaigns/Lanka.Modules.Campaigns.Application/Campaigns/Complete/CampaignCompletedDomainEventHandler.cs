using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;
using Lanka.Modules.Campaigns.Domain.Notifications;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Complete;

internal sealed class CampaignCompletedDomainEventHandler
    : DomainEventHandler<CampaignCompletedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CampaignCompletedDomainEventHandler(
        ISender sender,
        IEventBus eventBus,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._sender = sender;
        this._eventBus = eventBus;
        this._notificationRepository = notificationRepository;
        this._unitOfWork = unitOfWork;
    }

    public override async Task Handle(
        CampaignCompletedDomainEvent notification,
        CancellationToken cancellationToken = default
    )
    {
        Result<CampaignResponse> result =
            await this._sender.Send(new GetCampaignQuery(notification.CampaignId.Value), cancellationToken);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetCampaignQuery), CampaignErrors.NotFound);
        }

        this._notificationRepository.Add(Notification.Create(
            new BloggerId(result.Value.CreatorId),
            result.Value.Id,
            "Campaign completed",
            $"Campaign '{result.Value.Name}' has been completed by the client",
            new DateTimeOffset(notification.OccurredOnUtc, TimeSpan.Zero)
        ));
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        await this._eventBus.PublishAsync(
            new CampaignNotificationIntegrationEvent(
                Guid.CreateVersion7(),
                notification.OccurredOnUtc,
                result.Value.CreatorId,
                result.Value.Id,
                result.Value.Name,
                "Completed"
            ),
            cancellationToken
        );

        await this._eventBus.PublishAsync(
            new CampaignCompletedIntegrationEvent(
                notification.Id,
                notification.OccurredOnUtc,
                notification.CampaignId.Value,
                result.Value.OfferId,
                result.Value.ClientId,
                result.Value.CreatorId,
                notification.CompletedAtUtc
            ),
            cancellationToken
        );
    }
}
