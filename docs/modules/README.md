# 🧩 Lanka Modules Documentation

<div align="center">

*Deep dive into the modular architecture that powers Lanka's social media campaign management platform*

**"Good architecture makes the system easy to understand, develop, maintain, and deploy."**

[![Modules](https://img.shields.io/badge/Modules-4%20Active-blue?style=for-the-badge)](.)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%26%20Modular-green?style=for-the-badge)](../architecture/)
[![Domain-Driven](https://img.shields.io/badge/DDD-Domain--Driven-purple?style=for-the-badge)](../catalog-of-terms/)

</div>

---

## 🗺️ **Module Ecosystem Overview**

Lanka's modular monolith is built around **four core modules**, each responsible for a specific business domain. This structure provides clear boundaries, independent evolution, and maintainable code organization.

```mermaid
graph TB
    subgraph "🌐 Presentation Layer"
        API[Lanka.Api<br/>🚪 Gateway & Orchestration]
        WEB[Client Applications<br/>🖥️ Angular SPA]
    end
    
    subgraph "🧩 Business Modules"
        subgraph "👥 Users Module"
            UA[Users.Application<br/>🎯 Identity & Access]
            UD[Users.Domain<br/>💎 User Management]
            UI[Users.Infrastructure<br/>🗃️ Authentication]
            UP[Users.Presentation<br/>🌐 User APIs]
        end
        
        subgraph "📊 Analytics Module"
            AA[Analytics.Application<br/>📈 Business Logic]
            AD[Analytics.Domain<br/>🎭 Instagram Entities]
            AI[Analytics.Infrastructure<br/>🔗 Instagram API]
            AP[Analytics.Presentation<br/>🌐 Analytics APIs]
        end
        
        subgraph "🎪 Campaigns Module"
            CA[Campaigns.Application<br/>🚀 Campaign Logic]
            CD[Campaigns.Domain<br/>🎯 Business Rules]
            CI[Campaigns.Infrastructure<br/>💼 Data Layer]
            CP[Campaigns.Presentation<br/>🌐 Campaign APIs]
        end
        
        subgraph "🔍 Matching Module"
            MA[Matching.Application<br/>🔍 Search Logic]
            MD[Matching.Domain<br/>🎯 Search Models]
            MI[Matching.Infrastructure<br/>⚡ Search Engine]
            MP[Matching.Presentation<br/>🌐 Search APIs]
        end
    end
    
    subgraph "🔧 Shared Infrastructure"
        COMMON[Common.Infrastructure<br/>🛠️ Cross-cutting Concerns]
        DB[(PostgreSQL<br/>🗃️ Primary Database)]
        MONGO[(MongoDB<br/>📊 Analytics Storage)]
        REDIS[(Redis<br/>⚡ Caching & Sessions)]
        RABBIT[(RabbitMQ<br/>📮 Message Bus)]
    end
    
    API --> UA & AA & CA & MA
    WEB --> API
    UA & AA & CA & MA --> UD & AD & CD & MD
    UD & AD & CD & MD --> UI & AI & CI & MI
    UI & AI & CI & MI --> COMMON
    UP & AP & CP & MP --> API
    COMMON --> DB & MONGO & REDIS & RABBIT
    
    classDef moduleStyle fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef infraStyle fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef dataStyle fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    
    class UA,UD,UI,UP,AA,AD,AI,AP,CA,CD,CI,CP,MA,MD,MI,MP moduleStyle
    class API,COMMON infraStyle
    class DB,MONGO,REDIS,RABBIT dataStyle
```

---

## 🎯 **Core Modules Overview**

<table>
<tr>
<td width="25%">

### **👥 [Users Module](users/)**
*Identity & Access Management*

**Key Features:**
- 🔐 Authentication & Authorization
- 👤 User Profile Management  
- 🔗 Instagram Account Linking
- 📊 User Activity Tracking

**Core Entities:**
- User (Aggregate Root)
- Role, Permission
- Email, BirthDate

</td>
<td width="25%">

### **📊 [Analytics Module](analytics/)**
*Social Media Intelligence*

**Key Features:**
- 📈 Instagram Analytics
- 👥 Audience Insights
- 📊 Performance Metrics
- 🔄 Real-time Data Sync

**Core Entities:**
- InstagramAccount (Entity)
- Statistics, Audience
- Token, Metadata

</td>
<td width="25%">

### **🎪 [Campaigns Module](campaigns/)**
*Campaign Orchestration*

**Key Features:**
- 🚀 Campaign Management
- 👥 Blogger Network
- 💼 Offer Management
- 📋 Contract & Review System

**Core Entities:**
- Campaign (Entity)
- Blogger, Offer
- Pact, Review

</td>
<td width="25%">

### **🔍 [Matching Module](matching/)**
*Search & Discovery*

**Key Features:**
- 🔍 Advanced Search
- 🎯 Content Matching
- 📊 Relevance Scoring
- ⚡ Fast Indexing

**Core Entities:**
- SearchableItem
- MatchingCriteria
- MatchResult

</td>
</tr>
</table>

---

## 🏗️ **Module Architecture Patterns**

### **📐 Clean Architecture Layers**

Each module follows the **Clean Architecture** pattern with consistent layer organization:

```
📁 Lanka.Modules.{Module}.Domain/
   ├── 🎭 Entities & Aggregates
   ├── 💎 Value Objects  
   ├── ⚡ Domain Events
   ├── 🔍 Domain Services
   └── 📋 Repository Interfaces

📁 Lanka.Modules.{Module}.Application/
   ├── 🎯 Use Cases (Commands/Queries)
   ├── 🔄 Event Handlers
   ├── 📝 DTOs & Contracts
   ├── 🧩 Application Services
   └── 🔧 Abstractions

📁 Lanka.Modules.{Module}.Infrastructure/
   ├── 🗃️ Repository Implementations
   ├── 🔗 External Service Integrations
   ├── 📤 Outbox Pattern
   ├── 📥 Inbox Pattern
   └── 🗄️ Database Configuration

📁 Lanka.Modules.{Module}.Presentation/
   ├── 🌐 API Endpoints
   ├── 🔒 Permission Definitions
   ├── 📊 Response Models
   └── 🏷️ API Tags

📁 Lanka.Modules.{Module}.IntegrationEvents/
   ├── 📡 Integration Events
   ├── 🔄 Event Handlers
   └── 📮 Cross-Module Communication
```

### **🔄 Communication Patterns**

<table>
<tr>
<td width="50%">

#### **🔗 Intra-Module Communication**
- **Direct method calls** within the same module
- **Domain events** for internal business logic
- **Repository pattern** for data access
- **Mediator pattern** for use case orchestration

</td>
<td width="50%">

#### **📡 Inter-Module Communication**
- **Integration events** via RabbitMQ
- **Outbox/Inbox patterns** for reliability
- **Eventual consistency** for cross-module data
- **API calls** for synchronous operations

</td>
</tr>
</table>

---

## 📊 **Module Dependencies & Integration**

### **🔗 Module Interaction Matrix**

| From → To | 👥 Users | 📊 Analytics | 🎪 Campaigns | 🔍 Matching |
|-----------|----------|--------------|---------------|-------------|
| **👥 Users** | - | ✅ Account Linked | ✅ User Created | ✅ User Profile |
| **📊 Analytics** | ✅ User Activity | - | ✅ Data Updated | ✅ Content Indexed |
| **🎪 Campaigns** | ✅ Blogger Actions | ✅ Performance Data | - | ✅ Campaign Indexed |
| **🔍 Matching** | ❌ Read-only | ❌ Read-only | ❌ Read-only | - |

**Legend:**
- ✅ **Publishes events to**
- ❌ **No direct dependency** 

### **📮 Key Integration Events**

<table>
<tr>
<td width="33%">

#### **👥 Users Module Events**
- `UserCreatedIntegrationEvent`
- `UserDeletedIntegrationEvent` 
- `UserLoggedInIntegrationEvent`
- `InstagramAccountLinkedIntegrationEvent`

</td>
<td width="33%">

#### **📊 Analytics Module Events**
- `InstagramAccountDataFetchedIntegrationEvent`
- `InstagramAccountDataRenewedIntegrationEvent`
- `AnalyticsDataUpdatedIntegrationEvent`

</td>
<td width="33%">

#### **🎪 Campaigns Module Events**
- `CampaignCreatedIntegrationEvent`
- `CampaignCompletedIntegrationEvent`
- `BloggerJoinedIntegrationEvent`
- `ReviewSubmittedIntegrationEvent`

</td>
</tr>
</table>

---

## 🎨 **Design Principles & Best Practices**

### **💎 Domain-Driven Design**

<table>
<tr>
<td width="50%">

#### **🏛️ Aggregate Design**
- **Small aggregates** focused on business invariants
- **Eventual consistency** between aggregates
- **Rich domain models** with business logic
- **Domain events** for significant changes

#### **📝 Ubiquitous Language**
- **Shared vocabulary** between developers and domain experts
- **Consistent naming** across all layers
- **Business-focused** entity and method names

</td>
<td width="50%">

#### **🔒 Encapsulation & Validation**
- **Private setters** to control state changes
- **Factory methods** for object creation
- **Guard clauses** for input validation
- **Immutable value objects** where appropriate

#### **⚡ Event-Driven Architecture**
- **Domain events** for internal module communication
- **Integration events** for cross-module communication
- **Eventual consistency** for distributed operations

</td>
</tr>
</table>

### **🔧 Technical Excellence**

#### **📏 Code Quality Standards**
- **SOLID principles** in all implementations
- **Repository pattern** for data access abstraction
- **CQRS pattern** for command/query separation
- **Result pattern** for error handling

#### **🧪 Testing Strategy**
- **Unit tests** for domain logic
- **Integration tests** for full workflows
- **Architecture tests** to enforce design rules
- **Contract tests** for API consistency

---

## 🚀 **Getting Started with Modules**

### **📖 Learning Path**

1. **🎯 Start Here**: [Architecture Overview](../architecture/) - Understand the big picture
2. **👥 Begin with Users**: [Users Module](users/) - Authentication and identity
3. **📊 Add Analytics**: [Analytics Module](analytics/) - Social media intelligence  
4. **🎪 Build Campaigns**: [Campaigns Module](campaigns/) - Campaign management
5. **🔍 Implement Search**: [Matching Module](matching/) - Advanced search capabilities

### **🛠️ Development Workflow**

<table>
<tr>
<td width="50%">

#### **🆕 Adding New Features**
1. **Define domain model** in Domain layer
2. **Create use cases** in Application layer
3. **Implement data access** in Infrastructure layer
4. **Expose APIs** in Presentation layer
5. **Add integration events** if needed

</td>
<td width="50%">

#### **🔄 Modifying Existing Features**
1. **Start with domain model** changes
2. **Update use cases** accordingly
3. **Modify repository** implementations
4. **Update API contracts** if needed
5. **Handle backward compatibility**

</td>
</tr>
</table>

---

## 📚 **Module Documentation Index**

<div align="center">

| Module | Status | Documentation | Key Features |
|--------|--------|---------------|-------------|
| 👥 **[Users](users/)** | ✅ Active | [📖 Complete](users/) | Authentication, Profiles, Instagram Linking |
| 📊 **[Analytics](analytics/)** | ✅ Active | [📖 Complete](analytics/) | Instagram Analytics, Audience Insights |  
| 🎪 **[Campaigns](campaigns/)** | ✅ Active | [📖 Complete](campaigns/) | Campaign Management, Blogger Network |
| 🔍 **[Matching](matching/)** | 🚧 Development | [📖 In Progress](matching/) | Search, Content Discovery |

</div>

---

## 🎯 **Quick Links**

<div align="center">

[![Users Module](https://img.shields.io/badge/👥-Users%20Module-blue?style=for-the-badge)](users/)
[![Analytics Module](https://img.shields.io/badge/📊-Analytics%20Module-green?style=for-the-badge)](analytics/)
[![Campaigns Module](https://img.shields.io/badge/🎪-Campaigns%20Module-orange?style=for-the-badge)](campaigns/)
[![Matching Module](https://img.shields.io/badge/🔍-Matching%20Module-purple?style=for-the-badge)](matching/)

[![Architecture Guide](https://img.shields.io/badge/🏗️-Architecture-blue?style=for-the-badge)](../architecture/)
[![Catalog of Terms](https://img.shields.io/badge/📚-Terms-green?style=for-the-badge)](../catalog-of-terms/)
[![ADR](https://img.shields.io/badge/🎯-Decisions-orange?style=for-the-badge)](../architecure-decision-log/)

</div>

---

<div align="center">

*"A well-designed module is like a well-written book chapter - it tells a complete story while being part of a larger narrative."*

**Happy Coding! 🚀**

</div>

