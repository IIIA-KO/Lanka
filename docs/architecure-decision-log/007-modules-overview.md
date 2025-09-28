# ADL 007 - Overall Module Architecture and Domain Model Overview

**Date:** 2025-02-03  
**Status:** Accepted

## Context

The Lanka project is an evolution of Trendlink, designed to connect Instagram influencers and small-to-medium businesses for advertising collaborations. Given the complexity of the domain, I have chosen to decompose the system into several modules. This ADL documents initial "helicopter view" of the overall architecture, including key modules, their aggregates, domain events, business logic, and rules. Note that this is an evolving baseline; future business requirements or technical insights may necessitate changes to this model.

## Decision

Adopt a modular architecture that decomposes the domain into the following areas:

## Module Overview

The Lanka system is organized into the following key modules, each with a clear focus and responsibility. These modules represent the starting point for development and may evolve over time as requirements and technical considerations change.

### 1. **Users**

- **Purpose:** Handle user registration, authentication, and profile management, including linking Instagram accounts.
- **Key Responsibilities:**
  - Managing user aggregates and domain events such as `UserCreated` and `InstagramLinked`.
  - Integration with Keycloak for secure authentication.

### 2. **Analytics**

- **Purpose:** Collect, process, and store analytical data from external sources (e.g., Instagram) for analysis and insights.
- **Key Responsibilities:**
  - Fetching and processing Instagram statistics through API integration.
  - Generating domain events like `DataIngested`.

### 3. **Matching**

- **Purpose:** Implement advanced search capabilities, content matching algorithms, and intelligent recommendations for connecting bloggers and brands for collaborations.
- **Key Responsibilities:**
  - Advanced search functionality across campaigns, influencers, offers, and reviews.
  - Content indexing and relevance scoring.
  - ML-powered recommendations and personalized suggestions.
  - Generating domain events such as `ContentIndexed` and `RecommendationGenerated`.

### 4. **Campaigns**

- **Purpose:** Manage advertising campaigns, including their creation, updates, and cancellations.
- **Key Responsibilities:**
  - Processing campaign operations.
  - Generating domain events like `CampaignCreated`, `CampaignUpdated`, and `CampaignCancelled`.

### 5. **Communications**

- **Purpose:** Handle notifications and messaging between users and the system.
- **Key Responsibilities:**
  - Filtering and processing events through the Outbox pattern.
  - Delivering notifications and messages to users.

### 6. **Integrations**

- **Purpose:** Facilitate interaction with external services such as Keycloak for authentication and Instagram API for data retrieval.
- **Key Responsibilities:**
  - Ensuring secure and reliable communication with external systems.
  - Synchronizing data with third-party platforms.

## Overall Mermaid Diagram

The following diagram illustrates the initial view of the domain model:

![OverallArchitectureDiagram](/docs/images/overall-architecture-diagram.png)

## Consequences

**Benefits:**

- Provides a comprehensive overview of the domain structure, clarifying the relationships between modules, events, and business rules.
- Establishes a solid baseline for development and future iterations.
- Enhances communication among team members and stakeholders by offering a clear, documented starting point.

**Risks:**

- As requirements evolve, this initial model may require significant changes.
- The complexity of inter-module relationships could introduce challenges if not managed carefully.

## Note

This diagram and its descriptions represent the initial ideas for the Lanka project. Although the implementation and business goals may evolve over time, this "helicopter view" will serve as the starting point and reference for ongoing development and architectural refinement.
It clearly states that while these ideas form the initial blueprint for Lanka, they are subject to evolution as the project matures.
