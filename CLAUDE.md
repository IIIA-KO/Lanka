# CLAUDE.md - Lanka Project Context

## 🌟 Project Overview

**Lanka** is a comprehensive social media campaign management platform that revolutionizes how influencers and brands connect, collaborate, and measure success. Built with modern architectural patterns and enterprise-grade reliability, Lanka provides the tools needed for data-driven influencer marketing.

### Core Value Propositions
- **📊 Exclusive Analytics**: Deep Instagram insights with granular audience demographics
- **🎯 Smart Matching**: AI-powered brand-influencer partnership discovery
- **🚀 Streamlined Collaboration**: Complete campaign lifecycle management

## 🏗️ Architecture

Lanka follows a **Modular Monolith** architecture pattern with Domain-Driven Design (DDD), CQRS, and Event-Driven Architecture principles.

### Technology Stack
- **.NET 9/10**: Primary backend framework with C# 12.0
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9.0**: ORM for data access
- **PostgreSQL 15+**: Primary database
- **MongoDB 7.0+**: Document storage for analytics
- **Redis 7.0+**: Caching and session management
- **RabbitMQ**: Message broker for event-driven communication
- **Docker**: Containerization platform
- **Keycloak**: Identity and access management

## 📁 Project Structure

```
Lanka/
├── src/
│   ├── Api/
│   │   ├── Lanka.Api/                    # Main API application
│   │   └── Lanka.Gateway/                # API Gateway
│   ├── Common/                           # Shared components
│   │   ├── Lanka.Common.Application/     # Application layer abstractions
│   │   ├── Lanka.Common.Contracts/       # Shared contracts
│   │   ├── Lanka.Common.Domain/          # Domain layer abstractions
│   │   ├── Lanka.Common.Infrastructure/  # Infrastructure abstractions
│   │   ├── Lanka.Common.IntegrationEvents/ # Event contracts
│   │   └── Lanka.Common.Presentation/    # Presentation layer abstractions
│   ├── Modules/                          # Business modules
│   │   ├── Analytics/                    # Instagram analytics & insights
│   │   ├── Campaigns/                    # Campaign management
│   │   ├── Matching/                     # Brand-influencer matching
│   │   └── Users/                        # User management & authentication
│   └── Tools/
│       └── Lanka.Seeder/                 # Database seeding tool
├── docs/                                 # Comprehensive documentation
├── docker-compose.yml                    # Development environment
└── Lanka.slnx                          # Solution file
```

### Module Architecture
Each module follows Clean Architecture principles:
- **Domain**: Core business logic and entities
- **Application**: Use cases, commands, queries (CQRS)
- **Infrastructure**: Data access, external services
- **Presentation**: API endpoints and contracts

## 🎯 Current Development Status

| Module | Status | Features | Completion |
|--------|--------|----------|------------|
| 👥 **Users** | ✅ Complete | Authentication, Profiles, Instagram Linking | 100% |
| 📊 **Analytics** | 🚧 In Progress | Data Collection, Processing, Visualization | 75% |
| 🎪 **Campaigns** | 🚧 In Progress | Creation, Management, Tracking | 60% |
| 🌐 **API Gateway** | ✅ Complete | Routing, Authentication, Rate Limiting | 100% |
| 📱 **Frontend** | 🎯 Planned | Angular SPA, Mobile Apps | 0% |

## 🛠️ Development Environment

### Prerequisites
- .NET 9/10 SDK
- Docker & Docker Compose
- PostgreSQL client (optional)

### Quick Start
```bash
# Start infrastructure services
docker-compose up -d

# Apply database migrations
cd src/Modules/Users/Lanka.Modules.Users.Infrastructure && dotnet ef database update
cd ../../../Analytics/Lanka.Modules.Analytics.Infrastructure && dotnet ef database update
cd ../../../Campaigns/Lanka.Modules.Campaigns.Infrastructure && dotnet ef database update

# Run the main API
cd ../../../../Api/Lanka.Api && dotnet run

# Run the API Gateway
cd ../Lanka.Gateway && dotnet run
```

### Service Ports
- **API**: http://localhost:4307
- **Gateway**: http://localhost:4308
- **Health Checks**: http://localhost:4307/healthz
- **Seq Logging**: http://localhost:8081

## 🔑 Key Concepts

### Domain-Driven Design
- **Aggregates**: User, Campaign, InstagramAccount, Blogger
- **Value Objects**: Demographics, EngagementMetrics, CampaignStatus
- **Domain Events**: UserRegistered, CampaignCreated, AnalyticsProcessed

### CQRS Pattern
- **Commands**: Create, Update, Delete operations
- **Queries**: Read operations with optimized data projections
- **Handlers**: Separate command and query handlers

### Event-Driven Architecture
- **Integration Events**: Cross-module communication
- **Domain Events**: Internal module events
- **Event Store**: RabbitMQ for reliable messaging

### Authentication & Authorization
- **Keycloak Integration**: Enterprise-grade identity provider
- **JWT Tokens**: Stateless authentication
- **Role-Based Access**: Influencer, Brand, Admin roles

## 📊 Instagram Analytics Features

### Real-time Data Collection
- **Account Insights**: Follower growth, reach, impressions
- **Audience Demographics**: Age, gender, location distribution
- **Engagement Metrics**: Likes, comments, shares, saves
- **Content Performance**: Post analytics, story metrics

### Data Processing Pipeline
- **MongoDB Storage**: Raw analytics data
- **PostgreSQL**: Processed insights and reports
- **Redis Caching**: Fast data retrieval

### Mock Services (Development)
- **MockInstagramAccountsService**: Simulated account data
- **MockInstagramAudienceService**: Demographic simulation
- **MockInstagramPostService**: Content analytics
- **MockInstagramStatisticsService**: Performance metrics

## 🎪 Campaign Management

### Campaign Lifecycle
1. **Creation**: Brand defines campaign parameters
2. **Discovery**: AI-powered influencer matching
3. **Application**: Influencers apply to campaigns
4. **Selection**: Brand reviews and selects influencers
5. **Execution**: Content creation and approval
6. **Tracking**: Performance monitoring
7. **Completion**: Final reporting and payments

### Key Entities
- **Campaign**: Core campaign information
- **Blogger**: Influencer profile with analytics
- **Application**: Influencer campaign applications
- **Contract**: Legal agreements and terms

## 🧪 Testing Strategy

### Architecture Tests
- **NetArchTest**: Enforces architectural boundaries
- **Module Isolation**: Prevents circular dependencies
- **Clean Architecture**: Validates layer dependencies

### Unit Tests
- **xUnit Framework**: Primary testing framework
- **Test Doubles**: Mocks and fakes for external dependencies
- **Coverage**: Comprehensive business logic testing

### Integration Tests
- **End-to-End**: Full workflow testing
- **Database Tests**: Entity Framework integration
- **API Tests**: HTTP endpoint validation

## 🚀 Deployment

### Docker Support
- **Multi-stage Builds**: Optimized production images
- **Health Checks**: Container monitoring
- **Environment Configuration**: Flexible deployment options

### Infrastructure Services
- **PostgreSQL**: Primary data persistence
- **MongoDB**: Analytics data storage
- **Redis**: Caching and sessions
- **RabbitMQ**: Message queuing
- **Keycloak**: Identity management
- **Seq**: Structured logging

## 📝 Development Guidelines

### Code Standards
- **Clean Code**: SOLID principles and clean architecture
- **Domain-Driven Design**: Rich domain models
- **CQRS**: Command-query separation
- **Event Sourcing**: Event-driven state changes

### Git Workflow
- **Feature Branches**: `feature/123-add-user-authentication`
- **Conventional Commits**: Structured commit messages
- **Pull Requests**: Code review required
- **CI/CD**: Automated testing and deployment

### Documentation
- **Architecture Decisions**: ADR documentation
- **API Documentation**: OpenAPI/Swagger specs
- **Domain Documentation**: Business logic explanations
- **Setup Guides**: Development environment setup

## 🔧 Common Development Tasks

### Database Operations
```bash
# Add migration
dotnet ef migrations add <MigrationName> -p <InfrastructureProject>

# Update database
dotnet ef database update -p <InfrastructureProject>

# Generate SQL script
dotnet ef script -p <InfrastructureProject>
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/Modules/Users/test/Lanka.Modules.Users.UnitTests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Docker Operations
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f <service-name>

# Rebuild services
docker-compose up --build
```

## 📚 Additional Resources

- **[📖 Full Documentation](docs/README.md)**: Comprehensive project documentation
- **[🏗️ Architecture Guide](docs/architecture/README.md)**: System design and patterns
- **[🚀 Quick Start](docs/development/quick-start.md)**: Development setup guide
- **[🤝 Contributing](CONTRIBUTING.md)**: Contribution guidelines
- **[📋 ADRs](docs/architecture-decision-log/README.md)**: Architecture decisions

## 🎯 Current Focus Areas

### In Development
- **Analytics Module**: Instagram data processing pipeline
- **Campaign Workflow**: Complete campaign lifecycle
- **Frontend Integration**: API preparation for UI

### Planned Features
- **Machine Learning**: Advanced influencer-brand matching
- **Mobile Apps**: iOS and Android applications
- **Advanced Analytics**: Predictive insights and recommendations
- **Payment Integration**: Automated campaign payments

---

**Note**: This is an active development project with frequent updates. Check the documentation for the latest architectural decisions and implementation details.