# 🚀 Lanka Quick Start Guide

<div align="center">

*Get up and running with Lanka development in under 10 minutes!*

</div>

---

## ⚡ **Prerequisites Checklist**

Before diving in, make sure you have these tools installed:

### **Required Tools**
- [ ] **.NET 10.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- [ ] **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
- [ ] **Git** - [Download here](https://git-scm.com/downloads)

### **Recommended Tools**
- [ ] **Visual Studio 2022** or **JetBrains Rider** or **VS Code**
- [ ] **Postman** or **Insomnia** for API testing
- [ ] **DBeaver** or **pgAdmin** for database management
- [ ] **Redis Desktop Manager** for cache inspection
- [ ] **MongoDB Compass** for managing NoSQL data

### **Verify Installation**
```bash
# Check .NET version
dotnet --version
# Should return: 10.0.x

# Check Docker
docker --version
# Should return: Docker version 20.x.x or higher

# Check Git
git --version
# Should return: git version 2.x.x or higher
```

---

## 🎯 **Step 1: Clone & Setup**

### **1.1 Clone the Repository**
```bash
# Clone the repo
git clone https://github.com/IIIA-KO/lanka.git
cd lanka

# Verify structure
ls -la
# You should see: src/, docs/, tests/, docker-compose.yml, etc.
```

### **1.2 Environment Setup**
```bash
# Copy environment template
cp .env.example .env

# Edit the .env file with your settings
# (Default values work for local development)
```

### **1.3 Restore Dependencies**
```bash
# Restore all NuGet packages
dotnet restore

# Verify build
dotnet build
# Should complete without errors
```

---

## 🐳 **Step 2: Start Infrastructure**

Lanka uses Docker Compose to run all infrastructure services locally.

### **2.1 Start All Services (as defined in docker-compose.yml)**
```bash
# Start infrastructure services exactly as configured
docker compose up -d

# Check running services
docker compose ps
```

You should see these services running:
- 🗃️ **PostgreSQL** (port 5432) - Primary database
- 📊 **MongoDB** (port 27017) - Analytics storage
- ⚡ **Redis** (port 6379) - Caching & sessions
- 🐰 **RabbitMQ** (port 5672, Management: 15672) - Message bus
- 🔐 **Keycloak** (port 18080) - Identity provider
- 📋 **Seq** (port 8081) - Centralized logging
- 🧠 **Elasticsearch** (port 9200) - Search index
- 📈 **Kibana** (port 5601) - Search/analytics UI
- 🔭 **Jaeger** (port 16686) - Distributed tracing

### **2.2 Verify Services**
```bash
# Check service health
curl http://localhost:15672  # RabbitMQ Management
curl http://localhost:18080  # Keycloak
curl http://localhost:8081   # Seq
curl http://localhost:9200   # Elasticsearch
curl http://localhost:16686  # Jaeger UI
```

### **2.3 Database Migrations (automatic on startup)**
No manual step is required. Each module applies EF Core migrations automatically on API startup.
You can still generate new migrations during development using the CLI:
```bash
# From repository root, for example Users module
dotnet ef migrations add <Name> --project src/Modules/Users/Lanka.Modules.Users.Infrastructure
```

---

## 🚀 **Step 3: Run the Application**

### **3.1 Start the API (port 4307)**
```bash
# Start the Lanka API
cd src/Api/Lanka.Api
dotnet run

# Or with hot reload for development
dotnet watch run
```

### **3.2 Verify API is Running**
Open your browser and navigate to:
- **🏠 API Base**: http://localhost:4307
- **🏥 Health Checks**: http://localhost:4307/healthz
- **🌐 Gateway**: http://localhost:4308

You should see the Lanka API documentation and health status.

### **3.3 Test API Endpoints**
```bash
# Test health endpoint
curl http://localhost:4307/healthz

# Expected response: "Healthy" with 200 status
```

---

## 🧪 **Step 4: Run Tests**

### **4.1 Unit Tests**
```bash
# Run all unit tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### **4.2 Integration Tests**
```bash
# Run integration tests (requires Docker services)
cd test/Lanka.IntegrationTests
dotnet test

# Run specific test category
dotnet test --filter Category=Users
```

### **4.3 Architecture Tests**
```bash
# Verify architecture rules
cd test/Lanka.ArchitectureTests
dotnet test
```

---

## 🎯 **Step 5: Your First API Call**

### **5.1 Register a User (through Users endpoints)**
```bash
curl -X POST http://localhost:4307/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "developer@lanka.com",
    "password": "DevPassword123!",
    "firstName": "Dev",
    "lastName": "User"
  }'
```

### **5.2 Login and Get Token**
```bash
curl -X POST http://localhost:4307/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "developer@lanka.com",
    "password": "DevPassword123!"
  }'
```

### **5.3 Access Protected Endpoint**
```bash
# Use the token from login response
curl -X GET http://localhost:4307/api/users/profile \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

---

## 🛠️ **Development Workflow**

### **Daily Development Setup**
```bash
# 1. Pull latest changes
git pull origin main

# 2. Start infrastructure (if not running)
docker-compose up -d

# 3. Update dependencies (if package changes)
dotnet restore

# 4. Run migrations (if schema changes)
# (Navigate to each module and run dotnet ef database update)

# 5. Start API with hot reload
cd src/Api/Lanka.Api
dotnet watch run
```

### **Creating a New Feature**
```bash
# 1. Create feature branch
git checkout -b feature/your-feature-name

# 2. Make your changes...

# 3. Run tests
dotnet test

# 4. Commit and push
git add .
git commit -m "feat: add your feature description"
git push origin feature/your-feature-name
```

---

## 🎪 **Module-Specific Development**

### **👥 Users Module**
```bash
# Navigate to Users module
cd src/Modules/Users/Lanka.Modules.Users.Infrastructure

# Create migration when you change schema (auto-applied on run)
dotnet ef migrations add YourMigrationName

# Run module tests
cd ../test/Lanka.Modules.Users.UnitTests
dotnet test
```

### **📊 Analytics Module**
```bash
# Navigate to Analytics module
cd src/Modules/Analytics/Lanka.Modules.Analytics.Infrastructure

# Work with MongoDB collections
# (Use MongoDB Compass or command line)

# Test Instagram API integration
cd ../test/Lanka.Modules.Analytics.IntegrationTests
dotnet test --filter Category=Instagram
```

### **🎯 Campaigns Module**
```bash
# Navigate to Campaigns module
cd src/Modules/Campaigns/Lanka.Modules.Campaigns.Infrastructure

# Test campaign workflows
cd ../test/Lanka.Modules.Campaigns.IntegrationTests
dotnet test --filter Category=Workflows
```

---

## 🐛 **Troubleshooting**

### **Common Issues**

**🔴 Docker Services Won't Start**
```bash
# Check Docker is running
docker info

# Reset Docker services
docker-compose down
docker-compose up -d --force-recreate
```

**🔴 Database Connection Errors**
```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Reset database
docker-compose down postgres
docker-compose up -d postgres

# Wait 30 seconds, then run migrations again
```

**🔴 Build Errors**
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

**🔴 Port Conflicts**
```bash
# Check what's using ports
netstat -an | grep :4307

# Kill processes or change ports in launchSettings.json
```

### **Need Help?**
- 📚 **Documentation**: [Full documentation](../README.md)
- 🐛 **Issues**: [Debugging guide](debugging.md)
- ❓ **FAQ**: [Common questions](faq.md)
- 💬 **Team Chat**: Reach out to the development team

---

## 🎉 **You're Ready!**

<div align="center">

**Congratulations! You now have Lanka running locally.** 🎊

Here's what you can do next:

[![Explore Architecture](https://img.shields.io/badge/🏗️-Explore%20Architecture-blue?style=for-the-badge)](../architecture/README.md)
[![Dev Setup](https://img.shields.io/badge/🛠️-Dev%20Setup-green?style=for-the-badge)](development-setup.md)
[![FAQ](https://img.shields.io/badge/❓-FAQ-orange?style=for-the-badge)](faq.md)

</div>

---

## 📋 **Development Checklist**

Track your progress with this handy checklist:

- [ ] ✅ **Environment Setup** - All tools installed and verified
- [ ] 🐳 **Infrastructure Running** - All Docker services up and healthy
- [ ] 🗃️ **Databases Initialized** - All migrations applied successfully
- [ ] 🚀 **API Running** - API accessible and Swagger UI working
- [ ] 🧪 **Tests Passing** - Unit and integration tests green
- [ ] 📡 **First API Call** - Successfully registered and authenticated user
- [ ] 🛠️ **Development Workflow** - Comfortable with daily development process
- [ ] 📚 **Documentation** - Familiar with docs structure and key resources

<div align="center">

*Happy coding! 🚀*

</div>
