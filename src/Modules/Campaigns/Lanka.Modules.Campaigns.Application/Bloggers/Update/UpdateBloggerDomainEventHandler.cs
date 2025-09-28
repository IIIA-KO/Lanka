using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Update;

internal sealed class UpdateBloggerDomainEventHandler
    : DomainEventHandler<BloggerUpdatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;

    public UpdateBloggerDomainEventHandler(
        ISender sender,
        IEventBus eventBus
    )
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        BloggerUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<BloggerResponse> result = await this._sender.Send(
            new GetBloggerQuery(domainEvent.BloggerId.Value),
            cancellationToken
        );
        
        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetBloggerQuery), result.Error);
        }
        
        await this._eventBus.PublishAsync(
            new BloggerUpdatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.BloggerId.Value,
                result.Value.FirstName,
                result.Value.LastName,
                result.Value.BirthDate,
                result.Value.Bio
            ),
            cancellationToken
        );
    }
}
