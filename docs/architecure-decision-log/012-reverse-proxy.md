# ADL 012 - Introduction of the Reverse Proxy Project

**Date:** 2025-05-03
**Status:** Accepted

## Context

The Lanka project requires a gateway to handle reverse proxying, resilience, rate limiting, and authentication. This gateway will act as a single entry point for the API and protect the backend services from abuse.

## Decision

A new project, `Lanka.Gateway`, will be introduced to act as the API gateway. This project will use the following technologies:

* **YARP (Yet Another Reverse Proxy):** For reverse proxying and routing requests to backend services.
* **Polly:** For resilience and fault handling.
* **Rate Limiting:** For controlling the rate of requests to the API.
* **JWT Authentication:** For authenticating requests.

## Consequences

### Positive

* Provides a single entry point for the API.
* Protects backend services from abuse.
* Improves the resilience of the system.
* Simplifies authentication and authorization.

### Negative

* Adds complexity to the system.
* Requires additional configuration and maintenance.
