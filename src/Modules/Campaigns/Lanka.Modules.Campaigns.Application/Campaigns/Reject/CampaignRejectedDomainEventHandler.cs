using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Chat;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;
using Lanka.Modules.Campaigns.Domain.Chat;
using Lanka.Modules.Campaigns.Domain.Notifications;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Reject;

internal sealed class CampaignRejectedDomainEventHandler
    : DomainEventHandler<CampaignRejectedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private readonly INotificationRepository _notificationRepository;
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CampaignRejectedDomainEventHandler(
        ISender sender,
        IEventBus eventBus,
        INotificationRepository notificationRepository,
        IChatThreadRepository chatThreadRepository,
        IChatMessageRepository chatMessageRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._sender = sender;
        this._eventBus = eventBus;
        this._notificationRepository = notificationRepository;
        this._chatThreadRepository = chatThreadRepository;
        this._chatMessageRepository = chatMessageRepository;
        this._unitOfWork = unitOfWork;
    }

    public override async Task Handle(
        CampaignRejectedDomainEvent notification,
        CancellationToken cancellationToken = default
    )
    {
        Result<CampaignResponse> result =
            await this._sender.Send(new GetCampaignQuery(notification.CampaignId.Value), cancellationToken);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetCampaignQuery), CampaignErrors.NotFound);
        }

        var occurredAt = new DateTimeOffset(notification.OccurredOnUtc, TimeSpan.Zero);

        this._notificationRepository.Add(Notification.Create(
            new BloggerId(result.Value.CreatorId),
            result.Value.Id,
            "Campaign rejected",
            $"Your campaign '{result.Value.Name}' has been rejected by the client",
            occurredAt
        ));
        Result<ChatThread> threadResult = await CampaignChatThreadResolver.GetOrCreateAsync(
            this._chatThreadRepository,
            result.Value,
            occurredAt,
            cancellationToken);

        if (threadResult.IsFailure)
        {
            throw new LankaException(nameof(CampaignChatThreadResolver), threadResult.Error);
        }

        this._chatMessageRepository.Add(ChatMessage.CreateSystemMessage(
            threadResult.Value.Id,
            "CAMPAIGN_REJECTED",
            occurredAt));

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        await this._eventBus.PublishAsync(
            new CampaignNotificationIntegrationEvent(
                Guid.CreateVersion7(),
                notification.OccurredOnUtc,
                result.Value.CreatorId,
                result.Value.Id,
                result.Value.Name,
                "Rejected"
            ),
            cancellationToken
        );

        await this._eventBus.PublishAsync(
            new CampaignRejectedIntegrationEvent(
                notification.Id,
                notification.OccurredOnUtc,
                notification.CampaignId.Value,
                result.Value.OfferId,
                result.Value.ClientId,
                result.Value.CreatorId
            ),
            cancellationToken
        );
    }
}
