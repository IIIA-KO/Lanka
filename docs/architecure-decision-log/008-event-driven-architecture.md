# Event-Driven Architecture Implementation - Update

**Date:** 2025-04-06
**Status:** Accepted

## Update Context

The original event-driven architecture has been enhanced with reliable messaging patterns to ensure at-least-once delivery and exactly-once processing semantics.

## Decision

In addition to the existing dual-event system, we have implemented:

1. **Outbox Pattern** - For reliable publication of domain events
2. **Inbox Pattern** - For reliable consumption of integration events

### Updated Implementation Details

1. **Domain Events with Outbox**

   Domain events are now:
   - Captured during entity changes using an EF Core interceptor
   - Stored in an outbox table within the same transaction
   - Processed asynchronously by a background job
   - Tracked for idempotent handling

2. **Integration Events with Inbox**

   Integration events are now:
   - Received by MassTransit consumers 
   - Stored in an inbox table before processing
   - Processed asynchronously by a background job
   - Tracked for idempotent handling

3. **Reliability Features**

   - Retry policy with configurable attempts and backoff
   - Dead letter queue for persistently failing messages
   - Message cleanup for housekeeping

See the dedicated ADL on Reliable Messaging for full details.

## Additional Consequences

### Positive

- Messages are never lost during processing
- Events are processed exactly once, even in failure scenarios
- System is resilient to temporary failures
- Visibility into message processing state and failures

### Negative

- Additional database tables and background processes
- Increased system complexity 
- Eventual consistency model (asynchronous processing)