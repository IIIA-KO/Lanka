<div align="center">

# 🌟 Lanka

**The Modern Social Media Campaign Management Platform**

*Connecting influencers, brands, and data-driven insights in one powerful ecosystem*

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-blue?style=for-the-badge)](docs/architecture/README.md)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**[🚀 Quick Start](docs/development/quick-start.md) • [📚 Documentation](docs/README.md) • [🏗️ Architecture](docs/architecture/README.md) • [🤝 Contributing](CONTRIBUTING.md)**

</div>

## 🎯 **What is Lanka?**

Lanka is a **comprehensive social media campaign management platform** that revolutionizes how influencers and brands connect, collaborate, and measure success. Built with modern architectural patterns and enterprise-grade reliability, Lanka provides the tools needed for data-driven influencer marketing.

### **🔥 Core Value Propositions**

<table>
<tr>
<td width="33%">

### **📊 Exclusive Analytics**
Access **deep Instagram insights** that go far beyond public metrics. Get granular audience demographics, engagement patterns, optimal posting times, and performance predictions that empower data-driven decisions.

</td>
<td width="33%">

### **🎯 Smart Matching**
Leverage **AI-powered algorithms** to discover perfect brand-influencer partnerships. Our advanced filtering considers audience overlap, engagement quality, brand alignment, and campaign performance history.

</td>
<td width="33%">

### **🚀 Streamlined Collaboration**
Manage entire campaign lifecycles from discovery to completion. Handle contracts, content approval, payment processing, and performance tracking — all in one intuitive platform.

</td>
</tr>
</table>

## 🛠️ **Technology Stack**

<div align="center">

### **Backend Infrastructure**
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)

### **Databases & Storage**
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-336791?style=flat-square&logo=postgresql&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-7.0+-47A248?style=flat-square&logo=mongodb&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7.0+-DC382D?style=flat-square&logo=redis&logoColor=white)

### **Infrastructure & DevOps**
![Docker](https://img.shields.io/badge/Docker-Container%20Platform-2496ED?style=flat-square&logo=docker&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Broker-FF6600?style=flat-square&logo=rabbitmq&logoColor=white)
![Keycloak](https://img.shields.io/badge/Keycloak-Identity%20Provider-blue?style=flat-square)

</div>

## 📊 **Project Status**

<div align="center">

### **🎯 Current Milestone: Core Platform Development**

| Module | Status | Features | Completion |
|--------|--------|----------|------------|
| 👥 **Users** | ✅ Complete | Authentication, Profiles, Instagram Linking | 100% |
| 📊 **Analytics** | 🚧 In Progress | Data Collection, Processing, Visualization | 75% |
| 🎪 **Campaigns** | 🚧 In Progress | Creation, Management, Tracking | 60% |
| 🌐 **API Gateway** | ✅ Complete | Routing, Authentication, Rate Limiting | 100% |
| 📱 **Frontend** | 🎯 Planned | Angular SPA, Mobile Apps | 0% |

</div>

## 🚀 **Getting Started**

### **⚡ Quick Start (5 minutes)**

```bash
# 1. Clone the repository
git clone https://github.com/your-org/lanka.git
cd lanka

# 2. Start infrastructure services
docker-compose up -d

# 3. Apply database migrations
cd src/Modules/Users/Lanka.Modules.Users.Infrastructure && dotnet ef database update
cd ../../../Analytics/Lanka.Modules.Analytics.Infrastructure && dotnet ef database update
cd ../../../Campaigns/Lanka.Modules.Campaigns.Infrastructure && dotnet ef database update

# 4. Run the API
cd ../../../../Api/Lanka.Api
dotnet run

# 5. Open your browser
# API: http://localhost:4307
# Health: http://localhost:4307/healthz
# Gateway: http://localhost:4308
# Seq: http://localhost:8081
```

### **📚 Comprehensive Setup**

For detailed setup instructions, environment configuration, and troubleshooting:

[![Quick Start Guide](https://img.shields.io/badge/🚀-Quick%20Start%20Guide-blue?style=for-the-badge)](docs/development/quick-start.md)
[![Development Setup](https://img.shields.io/badge/🛠️-Development%20Setup-green?style=for-the-badge)](docs/development/development-setup.md)
[![Troubleshooting](https://img.shields.io/badge/🐛-Troubleshooting-red?style=for-the-badge)](docs/development/faq.md)

## 📖 **Documentation**

Our documentation is designed to be **comprehensive**, **beautiful**, and **developer-friendly**:

### **🎯 Quick Navigation**

<table>
<tr>
<td width="50%">

#### **🚀 Getting Started**
- [⚡ Quick Start Guide](docs/development/quick-start.md)
- [🛠️ Development Setup](docs/development/development-setup.md)
- [❓ FAQ & Troubleshooting](docs/development/faq.md)

#### **🏗️ Architecture**
- [🧩 Architecture Overview](docs/architecture/README.md)
- [💎 DDD Decision](docs/architecure-decision-log/004-adoption-of-ddd.md)
- [🔄 Event-Driven Decision](docs/architecure-decision-log/008-event-driven-architecture.md)
- [🎪 CQRS Decision](docs/architecure-decision-log/005-cqrs-implementation.md)

</td>
<td width="50%">

#### **📚 Reference**
- [📖 Catalog of Terms](docs/catalog-of-terms/README.md)
- [🎯 Architecture Decisions](docs/architecure-decision-log/README.md)
- [🛠️ Development Tools](docs/tools/README.md)
// API development guide planned

#### **🎨 Visual Guides**
- [🗺️ System Architecture Diagrams](docs/architecture/README.md)
// Additional visualizations planned

</td>
</tr>
</table>

## 🤝 **Contributing**

We welcome contributions from developers of all skill levels! Lanka is built by the community, for the community.

### **🎯 How to Contribute**

<table>
<tr>
<td width="25%">

#### **🐛 Report Issues**
Found a bug or have a suggestion?
- [📝 Bug Report Template](https://github.com/your-org/lanka/issues/new?template=bug_report.md)
- [💡 Feature Request](https://github.com/your-org/lanka/issues/new?template=feature_request.md)

</td>
<td width="25%">

#### **💻 Code Contributions**
Ready to contribute code?
- [🚀 Contributing Guide](CONTRIBUTING.md)
- [📋 Development Workflow](docs/development/development-setup.md)

</td>
<td width="25%">

#### **📚 Documentation**
Help improve our docs:
- [✏️ Edit Documentation](docs/README.md)
- [🎨 Add Diagrams/Examples](docs/architecture/README.md)

</td>
<td width="25%">

#### **🧪 Testing**
Strengthen our test suite:
- [🧪 Testing Guidance](docs/development/faq.md#-testing-issues)

</td>
</tr>
</table>

### **📋 Contribution Guidelines**

- **🌿 Branch Naming**: `feature/123-add-user-authentication`
- **💬 Commit Messages**: [Conventional Commits](https://conventionalcommits.org/) format
- **🔍 Code Review**: All PRs require team review and approval
- **🧪 Testing**: Include tests for new features and bug fixes
- **📝 Documentation**: Update relevant documentation for changes

---

## 📞 **Support & Community**

### **💬 Get Help**
- 📚 **Documentation**: [Comprehensive Guides](docs/README.md)
- ❓ **FAQ**: [Common Questions](docs/development/faq.md)
- 🐛 **Issues**: [GitHub Issues](https://github.com/your-org/lanka/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/your-org/lanka/discussions)

---

## 📜 **License**

Lanka is released under the **MIT License**. See [LICENSE](LICENSE) for details.

---

<div align="center">

## 🌟 **Built with ❤️ by the Lanka Team**

*Empowering the future of influencer marketing through technology excellence*

[![View Documentation](https://img.shields.io/badge/📚-View%20Documentation-blue?style=for-the-badge)](docs/README.md)
[![Start Contributing](https://img.shields.io/badge/🤝-Start%20Contributing-green?style=for-the-badge)](CONTRIBUTING.md)
[![Join Community](https://img.shields.io/badge/💬-Join%20Community-purple?style=for-the-badge)](https://github.com/your-org/lanka/discussions)

**⭐ Star this repository if Lanka helps your development!**

---

*"Building bridges between influencers and brands, one line of code at a time."*

</div>
