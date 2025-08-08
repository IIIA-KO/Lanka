# 🌟 Lanka Documentation Wiki

<div align="center">

**A Modern Modular Monolith for Social Media Campaign Management**

[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-blue?style=for-the-badge)](architecure-decision-log/003-modular-monolith-architecture.md)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple?style=for-the-badge&logo=dotnet)](architecure-decision-log/002-technology-stack.md)
[![Domain-Driven Design](https://img.shields.io/badge/DDD-Enabled-green?style=for-the-badge)](architecure-decision-log/004-adoption-of-ddd.md)
[![Event-Driven](https://img.shields.io/badge/Event--Driven-Architecture-orange?style=for-the-badge)](architecure-decision-log/008-event-driven-architecture.md)

*Welcome to the Lanka project documentation - your comprehensive guide to understanding, developing, and extending the Lanka social media campaign management platform.*

</div>

---

## 🗺️ **Navigation Hub**

<table>
<tr>
<td width="50%">

### 🚀 **Getting Started**
- [🏗️ Project Architecture Overview](#-project-architecture)
- [⚡ Quick Start Guide](development/quick-start.md)
- [🛠️ Development Setup](development/development-setup.md)

### 🏛️ **Architecture & Design**
- [🏗️ Architecture Overview](architecture/README.md)

</td>
<td width="50%">

### 👨‍💻 **Developer Guides**
- [⚡ Quick Start](development/quick-start.md)
- [🛠️ Development Setup](development/development-setup.md)
- [❓ FAQ & Troubleshooting](development/faq.md)

### 📚 **Reference**
- [📖 Catalog of Terms](catalog-of-terms/README.md)
- [🎯 Architecture Decisions](architecure-decision-log/README.md)
- [🛠️ Tools & Utilities](tools/README.md)
- [❓ FAQ & Common Issues](development/faq.md)

</td>
</tr>
</table>

---

## 🏗️ **Project Architecture**

Lanka is built as a **modular monolith** that combines the simplicity of monolithic deployment with the clarity and maintainability of modular design.

```mermaid
graph TB
    subgraph "🌐 Presentation Layer"
        API[Lanka.Api<br/>🚪 Gateway & Orchestration]
        WEB[Client Applications<br/>🖥️ React/Angular SPA]
    end
    
    subgraph "🧩 Module Ecosystem"
        subgraph "👥 Users Module"
            UA[Users.Application<br/>🎯 Business Logic]
            UD[Users.Domain<br/>💎 Core Entities]
            UI[Users.Infrastructure<br/>🗃️ Data & External Services]
        end
        
        subgraph "📊 Analytics Module"
            AA[Analytics.Application<br/>📈 Instagram Analytics]
            AD[Analytics.Domain<br/>🎭 Social Media Entities]
            AI[Analytics.Infrastructure<br/>🔗 Instagram API Integration]
        end
        
        subgraph "🎪 Campaigns Module"
            CA[Campaigns.Application<br/>🚀 Campaign Management]
            CD[Campaigns.Domain<br/>🎯 Campaign Logic]
            CI[Campaigns.Infrastructure<br/>💼 Business Operations]
        end
    end
    
    subgraph "🔧 Shared Infrastructure"
        COMMON[Common.Infrastructure<br/>🛠️ Cross-cutting Concerns]
        DB[(PostgreSQL<br/>🗃️ Primary Database)]
        MONGO[(MongoDB<br/>📊 Analytics Storage)]
        REDIS[(Redis<br/>⚡ Caching & Sessions)]
        RABBIT[(RabbitMQ<br/>📮 Message Bus)]
    end
    
    API --> UA & AA & CA
    WEB --> API
    UA & AA & CA --> UD & AD & CD
    UD & AD & CD --> UI & AI & CI
    UI & AI & CI --> COMMON
    COMMON --> DB & MONGO & REDIS & RABBIT
    
    classDef moduleStyle fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef infraStyle fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef dataStyle fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    
    class UA,UD,UI,AA,AD,AI,CA,CD,CI moduleStyle
    class API,COMMON infraStyle
    class DB,MONGO,REDIS,RABBIT dataStyle
```

---

## 🎯 **Core Modules**

<div align="center">

| Module | Purpose | Key Features |
|--------|---------|--------------|
| 👥 **Users** | Identity & Access Management | Authentication, Authorization, User Profiles |
| 📊 **Analytics** | Social Media Intelligence | Instagram Analytics, Audience Insights, Performance Metrics |
| 🎪 **Campaigns** | Campaign Orchestration | Campaign Creation, Blogger Management, Content Planning |

</div>

---

## 🛠️ **Technology Stack**

<div align="center">

### **Backend Stack**
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=flat-square&logo=c-sharp)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?style=flat-square&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-9.0-512BD4?style=flat-square&logo=dotnet)

### **Databases & Storage**
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17+-336791?style=flat-square&logo=postgresql&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-8.0+-47A248?style=flat-square&logo=mongodb&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7.0+-DC382D?style=flat-square&logo=redis&logoColor=white)

### **Architecture Patterns**
![Domain-Driven Design](https://img.shields.io/badge/DDD-Domain--Driven%20Design-blue?style=flat-square)
![CQRS](https://img.shields.io/badge/CQRS-Command%20Query%20Separation-green?style=flat-square)
![Event Sourcing](https://img.shields.io/badge/Event--Driven-Architecture-orange?style=flat-square)

### **Infrastructure & DevOps**
![Docker](https://img.shields.io/badge/Docker-Container%20Platform-2496ED?style=flat-square&logo=docker&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Broker-FF6600?style=flat-square&logo=rabbitmq&logoColor=white)
![Keycloak](https://img.shields.io/badge/Keycloak-Identity%20Provider-blue?style=flat-square)

</div>

---

## 🎨 **Design Principles**

> *"Good architecture makes the system easy to understand, develop, maintain, and deploy."*

### **🧩 Modular Design**
- **Clear boundaries** between business domains
- **Independent deployment** capabilities
- **Shared infrastructure** for cross-cutting concerns

### **💎 Domain-Driven Design**
- **Rich domain models** that reflect business rules
- **Ubiquitous language** shared between developers and domain experts
- **Bounded contexts** that encapsulate business logic

### **🔄 Event-Driven Architecture**
- **Loose coupling** between modules through events
- **Eventual consistency** for cross-module operations
- **Scalable communication** patterns

### **🎯 Clean Architecture**
- **Dependency inversion** at all levels
- **Testable business logic** isolated from infrastructure
- **Framework independence** where possible

---

## 🌟 **What Makes Lanka Special?**

<table>
<tr>
<td width="50%">

### **🚀 Developer Experience**
- **Modern C# 12** with latest language features
- **Hot reload** support for rapid development
- **Comprehensive testing** with clear patterns
- **Rich debugging** experience with detailed logging

### **🏗️ Architecture Benefits**
- **Easy to understand** modular structure
- **Simple deployment** as single application
- **Database per module** with shared infrastructure
- **Event-driven** communication between modules

</td>
<td width="50%">

### **📈 Business Value**
- **Instagram Analytics** for influencer marketing
- **Campaign Management** for brand partnerships
- **User Management** for multi-tenant scenarios
- **Real-time insights** for data-driven decisions

### **🔧 Technical Excellence**
- **Production-ready** patterns and practices
- **Observability** with OpenTelemetry
- **Resilience** with circuit breakers and retries
- **Security** with OAuth2 and JWT tokens

</td>
</tr>
</table>

---

## 📖 **Documentation Structure**

This documentation is organized into several key areas:

- **🏗️ [Architecture](architecture/)** - Deep dives into system design and patterns
- **👨‍💻 [Development](development/)** - Practical guides for building features
- **📚 [Reference](catalog-of-terms/)** - Terminology and concepts
- **🎯 [Decisions](architecure-decision-log/)** - Architecture decision records
- **🛠️ [Tools](tools/)** - Utilities and helper documentation

---

## 🤝 **Contributing to Documentation**

We believe great documentation is a team effort! Here's how you can help:

1. **📝 Found a typo?** Submit a quick PR
2. **💡 Missing information?** Open an issue with suggestions
3. **🎨 Visual improvements?** Add diagrams or improve formatting
4. **📚 New guides?** Write tutorials for common tasks

---

## 🎯 **Quick Links**

<div align="center">

[![Get Started](https://img.shields.io/badge/🚀-Get%20Started-blue?style=for-the-badge)](development/quick-start.md)
[![View Architecture](https://img.shields.io/badge/🏗️-Architecture-green?style=for-the-badge)](architecture/README.md)
[![FAQ](https://img.shields.io/badge/❓-FAQ-orange?style=for-the-badge)](development/faq.md)
[![Troubleshooting](https://img.shields.io/badge/🐛-Troubleshooting-red?style=for-the-badge)](development/faq.md)

</div>

---

<div align="center">

*Made with ❤️ by the Lanka Development Team*

**Happy Coding! 🎉**

</div>