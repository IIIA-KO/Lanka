﻿using System.Collections.Concurrent;
using System.Reflection;
using Lanka.Common.Application.EventBus;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Common.Infrastructure.Inbox;

public static class IntegrationEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IIntegrationEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        Type[] integrationEventHandlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}-{type.Name}",
            _ =>
            {
                Type[] integrationEventHandlers = assembly.GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler<>).MakeGenericType(type)))
                    .ToArray();

                return integrationEventHandlers;
            });

        List<IIntegrationEventHandler> handlers = [];

        handlers
            .AddRange(
                integrationEventHandlerTypes
                    .Select(serviceProvider.GetRequiredService)
                    .Select(integrationEventHandler => (integrationEventHandler as IIntegrationEventHandler)!)
            );

        return handlers;
    }
}
