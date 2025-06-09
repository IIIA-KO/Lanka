# Saga (Orchestration patter)

## Definition

A Saga is a design pattern used to manage long-running, distributed transactions across multiple services or modules without relying on a centralized, blocking transaction manager. Instead of a single atomic operation, a saga is a sequence of local transactions, each followed by an event or message, and potentially a compensating action if a subsequent step fails.

In modular monoliths or microservice architectures, sagas are essential for maintaining data consistency and reliability across bounded contexts.

## Key Characteristics

1. Orchestration and Choreography
   - Orchestrated Sagas use a central controller (e.g., a state machine) to coordinate steps

   - Choreographed Sagas rely on services to react to events and perform their role independently

2. Asynchronous Flow

   - Each step emits or reacts to events

   - No distributed locks or two-phase commits

   - Supports eventual consistency

3. Compensation

   - If a step fails, a prior successful step may require a compensating action to rollback side effects

   - Compensations are explicit, domain-aware operations

4. State Tracking

   - Uses persistent state to track progress

   - State machine frameworks (like MassTransit) model each step as a discrete state

## Implementation Guidelines

1. Design Rules

   - Model each step as a command or event

   - Define events that represent step success or failure

   - Identify compensation logic early

   - Define clear correlation IDs to track saga instances

2. Technical Considerations

   - Use durable storage for saga state (e.g., PostgreSQL, MongoDB, Redis)

   - Ensure event idempotency to avoid duplicate effects

   - Handle timeouts and retry policies

   - Use composite events to coordinate multiple success paths

3. Best Practices

   - Keep sagas small and purpose-specific

   - Avoid coupling to internal service logic; use events and commands

   - Log transitions and errors explicitly

   - Use meaningful domain language for events and states

4. When to Use

   - Multi-step operations spanning modules or services

   - Orchestration between loosely coupled domains

   - Workflows that include failure handling and rollback

## Examples

### Model

![Saga](/docs/images/saga.jpg)

### Code

```csharp
public sealed class LinkInstagramSaga : MassTransitStateMachine<LinkInstagramState>
{
    public State LinkingStarted { get; init; }
    public State InstagramAccountDataFetched { get; init; }

    public Event<InstagramAccountLinkedIntegrationEvent> EventAccountLinked { get; init; }
    public Event<InstagramAccountDataFetchedIntegrationEvent> EventAccountDataFetched { get; init; }
    public Event InstagramLinkingCompleted { get; init; }

    public LinkInstagramSaga()
    {
        this.Event(() => this.EventAccountLinked, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventAccountDataFetched, c => c.CorrelateById(m => m.Message.UserId));

        this.InstanceState(s => s.CurrentState);

        this.Initially(
            this.When(this.EventAccountLinked)
                .Publish(context =>
                    new InstagramAccountLinkingStartedIntegrationEvent(
                        context.Message.Id,
                        context.Message.OccurredOnUtc,
                        context.Message.UserId,
                        context.Message.Code
                    )
                )
                .TransitionTo(this.LinkingStarted)
        );

        this.During(this.LinkingStarted,
            this.When(this.EventAccountDataFetched)
                .TransitionTo(this.InstagramAccountDataFetched)
        );

        this.CompositeEvent(
            () => this.InstagramLinkingCompleted,
            state => state.LinkingCompletedStatus,
            this.EventAccountDataFetched
        );

        this.DuringAny(
            this.When(this.InstagramLinkingCompleted)
                .Publish(context =>
                    new InstagramAccountLinkingCompletedIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId
                    )
                )
                .Finalize()
        );
    }
}
```


## Additional References

1. [Saga Pattern](https://microservices.io/patterns/data/saga.html)