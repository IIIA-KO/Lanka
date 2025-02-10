# Integration Event

## Definition

An Integration Event is a message that represents a fact that has occurred within one module and needs to be communicated to other modules or external systems. Unlike Domain Events which are internal to a module, Integration Events facilitate cross-module or cross-system communication while maintaining loose coupling.

## Key Characteristics

### 1. Event Properties

- Unique identifier for tracing
- Timestamp of occurrence
- Version information for compatibility
- Serializable payload
- Module source information

### 2. Event Nature

- Cross-module communication
- Eventual consistency support
- Asynchronous processing
- Durable messaging

### 3. Event Handling

- Supports multiple subscribers
- Cross-boundary communication
- Idempotent processing
- Retry mechanisms

### 4. Event Publishing

- Published after transaction completion
- Guaranteed delivery
- Order preservation (when needed)
- Error handling and dead-letter support

## Implementation Guidelines

### 1. Design Rules

- Minimize payload size
- Consider serialization format

### 2. Technical Considerations

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

### 3. Best Practices

- Use meaningful event names
- Include correlation IDs
- Document event contracts
- Consider event schema evolution

## Examples

### Model

![IntegrationEvent](/docs/images/integration-event.jpg)

### Code

```csharp
public sealed class UserRegisteredIntegrationEvent : IntegrationEvent
{
    public UserRegisteredIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc,
        Guid userId,
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate
    ) 
        : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Email = email;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.BirthDate = birthDate;
    }
    
    public Guid UserId { get; init; }
    
    public string Email { get; init; }
    
    public string FirstName { get; init; }
    
    public string LastName { get; init; }
    
    public DateOnly BirthDate { get; init; }
}
```

```csharp
internal sealed class UserRegisteredIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    public override async Task Handle(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        // Handler implementation
    }
}
```

## Additional References

1. [Integration Events in Microservices](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications)
