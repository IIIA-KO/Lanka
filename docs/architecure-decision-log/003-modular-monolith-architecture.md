# ADL 003 - Modular Monolith Architecture

**Date:** 2025-02-01  
**Status:** Accepted

## Context

As Lanka is becoming a diffult project, it's required a clear separation of concerns while keeping the benefits of a single, unified codebase. A modular monolith offers the simplicity of deployment combined with the organizational benefits of isolated modules, which can later be split into microservices if needed.

## Decision

A structure of the application will be as a **Modular Monolith**. Key aspects include:

- Breaking down the system into distinct, independent modules (e.g., User Management, Analytics, Matching, etc.).
- Each module encapsulates its domain logic, data, and interfaces.
- Inter-module communication will be handled through well-defined contracts and, where applicable, domain events.

## Consequences

- **Benefits:**  
  - Improved maintainability and scalability of the codebase.  
  - Easier testing and isolation of business logic.  
  - Flexibility to later transition individual modules into microservices if required.
- **Risks:**  
  - Overhead in establishing clear module boundaries.  
  - Potential for increased complexity if inter-module communication if not managed carefully.
