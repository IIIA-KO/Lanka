using System.Collections.Concurrent;
using System.Reflection;
using Lanka.Common.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Common.Infrastructure.Outbox;

public static class DomainEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IDomainEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly
    )
    {
        Type[] domainEventHandlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{type.Name}",
            _ =>
            {
                Type[] domainEventHandlerTypes = assembly.GetTypes()
                    .Where(
                        t => t.IsAssignableTo(typeof(IDomainEventHandler<>).MakeGenericType(type))
                    )
                    .ToArray();

                return domainEventHandlerTypes;
            }
        );

        List<IDomainEventHandler> handlers = [];
        handlers.AddRange(
            domainEventHandlerTypes
                .Select(serviceProvider.GetRequiredService)
                .Select(domainEventHandler => (domainEventHandler as IDomainEventHandler)!)
        );

        return handlers;
    }
}
