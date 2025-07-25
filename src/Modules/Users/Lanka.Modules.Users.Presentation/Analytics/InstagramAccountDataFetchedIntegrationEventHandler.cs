using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Users.Application.Instagram.FinishLinking;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using MediatR;

namespace Lanka.Modules.Users.Presentation.Analytics;

internal sealed class InstagramAccountDataFetchedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountDataFetchedIntegrationEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    
    public InstagramAccountDataFetchedIntegrationEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        InstagramAccountDataFetchedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new FinishInstagramLinkingCommand(
                integrationEvent.UserId,
                integrationEvent.Username,
                integrationEvent.ProviderId
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            await this._eventBus.PublishAsync(
                new InstagramLinkingFailedIntegrationEvent(
                    integrationEvent.Id,
                    integrationEvent.OccurredOnUtc,
                    integrationEvent.UserId,
                    result.Error.Description
                ),
                cancellationToken
            );
            
            throw new LankaException(nameof(FinishInstagramLinkingCommand), result.Error);
        }
    }
}
