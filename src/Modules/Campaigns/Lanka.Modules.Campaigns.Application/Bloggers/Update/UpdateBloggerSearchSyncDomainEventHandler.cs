using System.Globalization;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Update;

internal sealed class UpdateBloggerSearchSyncDomainEventHandler
    : DomainEventHandler<BloggerUpdatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private static readonly string[] _tags = ["blogger", "profile"];

    public UpdateBloggerSearchSyncDomainEventHandler(ISender sender, IEventBus eventBus)
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

        var integrationEvent = new BloggerSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.BloggerId.Value,
            SearchSyncOperation.Update,
            title: $"{result.Value.FirstName} {result.Value.LastName}",
            content: result.Value.Bio,
            _tags,
            metadata: new Dictionary<string, object>
            {
                {
                    "BirthDate",
                    result.Value.BirthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }
            }
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
