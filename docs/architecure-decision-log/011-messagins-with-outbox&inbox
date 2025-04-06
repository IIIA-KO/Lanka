# Reliable Messaging with Outbox and Inbox Patterns

**Date:** 2025-04-06  
**Status:** Accepted

## Context

The current event-driven architecture in Lanka requires a more robust messaging system to ensure:

1. Messages are never lost during processing
2. Messages are processed exactly once (avoiding duplicates)
3. Modules maintain transactional consistency when publishing and consuming events
4. System is resilient to temporary failures

## Decision

Implement the Outbox and Inbox patterns to achieve reliable messaging:

### Outbox Pattern

- Domain events are persisted in an outbox table within the same transaction as the entity changes
- Background job processes outbox messages asynchronously
- Events are marked as processed once successfully handled
- Failed messages can be retried

### Inbox Pattern 

- Integration events received from other modules are stored in an inbox table before processing
- Background job processes inbox messages asynchronously
- Idempotency is ensured by tracking processed message IDs

### Technical Implementation

1. **Message Storage**
   - `OutboxMessage` - Stores domain events before publishing
   - `InboxMessage` - Stores integration events before processing
   - Both include: ID, Type, Content, OccurredOnUtc, ProcessedOnUtc, Error, RetryCount, Status

2. **Background Processing**
   - Quartz.NET scheduled jobs for processing outbox and inbox messages
   - Configurable batch size and processing interval
   - Retry policy with exponential backoff
   - Dead letter queue for persistently failing messages
   - Cleanup job for processed messages

3. **Idempotency**
   - Consumer tracking tables: `outbox_message_consumers` and `inbox_message_consumers`
   - Decorator pattern for handlers to ensure exactly-once processing
   - Uses handler name + message ID to track processed messages

## Consequences

### Positive

- Reliable at-least-once delivery of messages
- Exactly-once processing via idempotency tracking
- Transactional consistency between data changes and event publishing
- System resilience through message persistence and retries
- Visibility into message processing with error tracking

### Negative

- Increased complexity in message handling system
- Additional database tables and queries
- Eventual consistency model (messages are processed asynchronously)
- Need to monitor DLQ for persistently failing messages