# ADL 014 - MongoDB Adoption for Analytics Module

**Date:** 2025-07-21  
**Status:** Accepted

The MongoDB implementation will be configured with:

- Write concern: `majority` for critical operations
- Read preference: `primaryPreferred` for analytics queries
- Indexing strategy:
  - Compound indexes for common query patterns
  - TTL indexes for data retention policies
  - Time-series collections for metrics data

## Related Decisions

This MongoDB adoption decision is closely related to other architectural decisions:

1. **Event-Driven Architecture (ADL-008)**: MongoDB integrates with our event-driven architecture through the outbox pattern, ensuring reliable event publication while maintaining data consistency.

2. **CQRS Implementation (ADL-005)**: MongoDB serves as an optimized read store for analytics queries, aligning with our CQRS approach by providing efficient read models.

3. **Modular Monolith (ADL-003)**: The Analytics module maintains its data sovereignty using MongoDB while communicating with other modules through well-defined contracts.

4. **Saga Pattern (ADL-013)**: MongoDB stores Instagram analytics data that's part of cross-module workflows, such as the Instagram account linking saga.

These relationships demonstrate how MongoDB fits into our broader architectural vision while maintaining module independence and data sovereignty.
**Status:** Accepted

## Context

The Analytics module in Lanka is responsible for collecting, processing, and storing analytical data from Instagram API and user activity tracking. This data has several characteristics that influence our storage choice:

1. **Schema Flexibility**: Instagram API data structure may change over time, and different types of user activities need different data structures.
2. **High Write Load**: Continuous stream of user activity events and periodic Instagram API data updates.
3. **Complex Querying**: Need to perform various analytical queries across different dimensions of data.
4. **Time-Series Nature**: Most analytics data is time-series based (user activities, post performances, engagement metrics).

## Decision

Adopt MongoDB as the primary database for the Analytics module with the following key implementations:

1. **Separate Collections**:
   - Audience Analytics Collections:
     - `audience-age-distribution` - For age demographics data
     - `audience-gender-distribution` - For gender demographics data
     - `audience-location-distribution` - For geographical distribution data
     - `audience-reach-distribution` - For reach metrics
   - Statistics Collections:
     - `engagement-statistics` - For engagement data tracking
     - `interaction-statistics` - For user interaction metrics
     - `metrics-statistics` - For various Instagram metrics
     - `overview-statistics` - For aggregated account statistics
   - Activity Tracking:
     - `user-activity` - For monitoring user behavior and system interactions

   All collections reside in the `analytics` database.

2. **Data Organization**:
   - Use document model for flexible schema evolution
   - Implement time-based sharding for efficient historical data management
   - Utilize MongoDB's aggregation pipeline for complex analytics computations

3. **Integration Pattern**:
   - Implement repository pattern with MongoDB driver
   - Use change streams for real-time analytics updates
   - Leverage MongoDB's TTL indexes for data retention policies

## Consequences

**Benefits:**

- Schema flexibility allows easy adaptation to Instagram API changes
- Better performance for analytics queries due to document model
- Native support for time-series data and aggregations
- Horizontal scalability for growing data volumes
- Built-in support for real-time data changes through change streams

**Risks:**

- Need for additional expertise in MongoDB operations
- Potential complexity in maintaining consistency with other modules using different databases
- Required monitoring of MongoDB performance and optimization
- Additional infrastructure costs for MongoDB cluster management

## Technical Details

The MongoDB implementation will be configured with:

- Write concern: `majority` for critical operations
- Read preference: `primaryPreferred` for analytics queries
- Indexing strategy:
  - Compound indexes for common query patterns
  - TTL indexes for data retention
  - Time-series collections for metrics data

Integration with the existing event-driven architecture will be maintained through the outbox pattern, ensuring reliable event publication while storing data in MongoDB.
