# Domain Event

## Definition

A Domain Event represents something significant that has happened in the domain. It is always described in the past tense as it represents a fact that has already occurred. Domain Events are immutable and capture the state change or occurrence at a specific point in time.

## Key Characteristics

### 1. Event Properties

- Unique identifier for traceability
- Timestamp of occurrence
- Relevant data about what happened
- Immutable after creation

### 2. Event Nature

- Represents past occurrence
- Named in past tense
- Captures state at time of occurrence
- Contains all necessary context

### 3. Event Handling

- Processed asynchronously
- Can trigger side effects
- Supports eventual consistency
- Enables system decoupling

### 4. Event Publishing

- Published by domain entities
- Collected at aggregate level
- Dispatched after transaction completion
- Supports event ordering

## Implementation Guidelines

### 1. Design Rules

- Keep events focused and minimal
- Include only necessary data
- Ensure event immutability
- Consider versioning strategy

### 2. Technical Considerations

- Implement proper serialization
- Handle event ordering
- Consider event persistence
- Plan for idempotency

### 3. Best Practices

- Use meaningful event names
- Include correlation IDs
- Consider event upgrading
- Document event schemas

## Examples

### Model

To be added

### Code

To be added

## Additional References

1. [Domain Events Design and Implementation](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
