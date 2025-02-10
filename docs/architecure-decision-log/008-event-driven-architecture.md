# Event-Driven Architecture Implementation

**Date:** 2025-02-10  
**Status:** Accepted

## Context

The system needs a reliable way to handle internal communication between modules while maintaining loose coupling. It needs to support both immediate domain events and integration events across module boundaries.

## Decision

Implementation of a dual-event system:

1. Domain Events - for immediate, in-process communication
2. Integration Events - for cross-module communication

### Implementation Details

1. **Domain Events**

   ```csharp
   public interface IDomainEvent
   {
       Guid Id { get; }

       DateTime OccurredOn { get; }
   }
   ```

2. **Integration Events**

   ```csharp
    public interface IIntegrationEvent
    {
        Guid Id { get; }
        
        DateTime OccurredOnUtc { get; }
    }
   
   public abstract class IntegrationEvent : IIntegrationEvent
   {
       public Guid Id { get; }
       public DateTime OccurredOn { get; }
   }
   ```

3. **Event Bus**

   ```csharp
   public interface IEventBus
   {
       Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
            where T : IIntegrationEvent;
   }
   ```

## Consequences

### Positive

- Loose coupling between modules
- Easy to add new event handlers
- Clear separation between immediate and integration events
- Testable event flow

### Negative

- Additional complexity in event handling
- Need to manage event versioning
- Potential performance overhead for event serialization
