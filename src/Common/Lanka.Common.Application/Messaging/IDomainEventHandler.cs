using Lanka.Common.Domain;
using MediatR;

namespace Lanka.Common.Application.Messaging;
#pragma warning disable CA1711
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent;
