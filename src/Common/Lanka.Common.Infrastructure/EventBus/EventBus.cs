using Lanka.Common.Application.EventBus;
using MassTransit;

namespace Lanka.Common.Infrastructure.EventBus;

internal sealed class EventBus : IEventBus
{
    private readonly IBus _bus;

    public EventBus(IBus bus)
    {
        this._bus = bus;
    }
        
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        await this._bus.Publish(integrationEvent, cancellationToken);
    }
}
