using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Application.Notifications;
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

namespace Lanka.Modules.Campaigns.Application.Campaigns.Cancel;

internal sealed class CampaignCancelledDomainEventHandler
    : DomainEventHandler<CampaignCancelledDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private readonly INotificationRepository _notificationRepository;
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _chatNotificationService;

    public CampaignCancelledDomainEventHandler(
        ISender sender,
        IEventBus eventBus,
        INotificationRepository notificationRepository,
        IChatThreadRepository chatThreadRepository,
        IChatMessageRepository chatMessageRepository,
        IUnitOfWork unitOfWork,
        IChatNotificationService chatNotificationService
    )
    {
        this._sender = sender;
        this._eventBus = eventBus;
        this._notificationRepository = notificationRepository;
        this._chatThreadRepository = chatThreadRepository;
        this._chatMessageRepository = chatMessageRepository;
        this._unitOfWork = unitOfWork;
        this._chatNotificationService = chatNotificationService;
    }

    public override async Task Handle(
        CampaignCancelledDomainEvent notification,
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
            "Campaign cancelled",
            $"Campaign '{result.Value.Name}' has been cancelled",
            occurredAt
        ));
        this._notificationRepository.Add(Notification.Create(
            new BloggerId(result.Value.ClientId),
            result.Value.Id,
            "Campaign cancelled",
            $"Campaign '{result.Value.Name}' has been cancelled",
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

        var systemMessage = ChatMessage.CreateSystemMessage(
            threadResult.Value.Id,
            "CAMPAIGN_CANCELLED",
            occurredAt
        );

        this._chatMessageRepository.Add(systemMessage);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        await this._chatNotificationService.SendMessageAsync(
            SystemChatNotificationFactory.Create(systemMessage),
            cancellationToken);

        await this._eventBus.PublishAsync(
            new CampaignNotificationIntegrationEvent(
                Guid.CreateVersion7(),
                notification.OccurredOnUtc,
                result.Value.CreatorId,
                result.Value.Id,
                result.Value.Name,
                "Cancelled"
            ),
            cancellationToken
        );

        await this._eventBus.PublishAsync(
            new CampaignNotificationIntegrationEvent(
                Guid.CreateVersion7(),
                notification.OccurredOnUtc,
                result.Value.ClientId,
                result.Value.Id,
                result.Value.Name,
                "Cancelled"
            ),
            cancellationToken
        );

        await this._eventBus.PublishAsync(
            new CampaignCancelledIntegrationEvent(
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
