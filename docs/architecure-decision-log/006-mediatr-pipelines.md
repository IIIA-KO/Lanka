# ADL 006 - MediatR Pipeline Behaviors

**Date:** 2024-02-02  
**Status:** Accepted

## Context

The application needs consistent cross-cutting concerns across all requests, including validation, logging, exception handling, and caching.

## Decision

Implement MediatR Pipeline Behaviors for cross-cutting concerns:

1. ValidationPipelineBehavior - Request validation
2. RequestLoggingPipelineBehavior - Consistent request logging
3. ExceptionHandlingPipelineBehavior - Global exception handling
4. QueryCachingPipelineBehavior - Caching for queries

## Consequences

**Benefits:**

- Consistent handling of cross-cutting concerns
- Separation of concerns
- Reusable pipeline components
- Centralized configuration

**Risks:**

Pipeline order importance

- Performance overhead
- Complexity in debugging
