# Unit of Work

## Definition

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates the writing out of changes. In Entity Framework Core, this pattern is implemented by the DbContext, which tracks changes made to entities and persists them to the database during SaveChanges.

## Key Characteristics

### 1. Transaction Management

- Maintains transaction boundary
- Ensures data consistency
- Coordinates multiple operations
- Handles concurrency

### 2. Change Tracking

- Tracks entity modifications
- Manages entity state
- Coordinates multiple repositories
- Handles relationships

### 3. EF Core Implementation

- Built into DbContext
- Automatic change tracking
- Relationship management
- Transaction support

### 4. Extended Features

- Domain event dispatch
- Outbox pattern support (future)
- Inbox pattern support (future)
- Concurrency handling

## Implementation Guidelines

### 1. Design Rules

- Single responsibility per transaction
- Clear transaction boundaries
- Proper error handling
- Consistent state management

### 2. Technical Considerations

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 3. Best Practices

- Use async operations
- Implement proper disposal
- Handle concurrency conflicts
- Consider performance implications
- Plan for future patterns (Outbox/Inbox)

## Examples

## Model

![EventBus](/docs/images/event-bus.jpg)

## Additional References

1. [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)
2. [EF Core Change Tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
