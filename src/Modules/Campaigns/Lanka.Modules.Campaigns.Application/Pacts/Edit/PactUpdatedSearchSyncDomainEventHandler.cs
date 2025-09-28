using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;
using Lanka.Modules.Campaigns.Domain.Pacts.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Pacts;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Pacts.Edit;

internal sealed class PactUpdatedSearchSyncDomainEventHandler
    : DomainEventHandler<PactUpdatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private static readonly string[] _tags = ["pact", "agreement"];

    public PactUpdatedSearchSyncDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        PactUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<PactResponse> result = await this._sender.Send(
            new GetBloggerPactQuery(domainEvent.PactId.Value),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetBloggerPactQuery), result.Error);
        }

        var integrationEvent = new PactSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.PactId.Value,
            SearchSyncOperation.Update,
            title: $"Pact {result.Value.Id}",
            content: result.Value.Content,
            _tags,
            metadata: new Dictionary<string, object>
            {
                { "BloggerId", result.Value.BloggerId.ToString() }
            }
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
