# Quick Start Guide

<div align="center">

*Get Lanka running locally*

</div>

---

## Prerequisites

Before you start, make sure you have:

- [ ] **.NET 10.0 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- [ ] **Docker Desktop** — [Download](https://www.docker.com/products/docker-desktop)
- [ ] **Git** — [Download](https://git-scm.com/downloads)

### Recommended Tools

- **IDE**: Visual Studio 2022, JetBrains Rider, or VS Code with C# extensions
- **API Testing**: Postman or Insomnia
- **Database Management**: DBeaver, pgAdmin, or MongoDB Compass

### Verify Installation

```bash
dotnet --version   # Should return 10.0.x
docker --version   # Should return Docker version 20.x or higher
git --version      # Should return git version 2.x or higher
```

---

## Step 1: Clone & Setup

```bash
# Clone the repo
git clone https://github.com/IIIA-KO/Lanka.git
cd Lanka

# Restore NuGet packages
dotnet restore

# Verify it builds
dotnet build
```

---

## Step 2: Start Everything with Aspire

Lanka uses .NET Aspire to orchestrate all infrastructure containers and application projects. One command starts everything.

### Prerequisites

```bash
# Install the Aspire workload (one-time setup)
dotnet workload install aspire
```

### Start

```bash
# Start all infrastructure + API + Gateway
dotnet run --project src/Api/Lanka.AppHost
```

Aspire starts these services automatically:
- **PostgreSQL** (5432) — Primary database
- **MongoDB** (27017) — Analytics storage
- **Redis** (6379) — Caching
- **RabbitMQ** (5672, Management: 15672) — Message bus
- **Keycloak** (18080) — Identity provider
- **Elasticsearch** — Search index
- **Kibana** (5601) — Elasticsearch UI
- **Lanka.Api** — Main application
- **Lanka.Gateway** (4308) — YARP reverse proxy

The console output includes a link to the **Aspire Dashboard** — a unified UI for logs, traces, and metrics.

### Verify Services

Open the Aspire Dashboard URL from the console output. All resources should show as **Running** with **Healthy** status. You can also check:

```bash
curl http://localhost:4307/healthz  # API health check
```

---

## Step 3: Database Migrations

Migrations are applied **automatically** when the API starts. No manual step needed.

If you need to generate new migrations during development:

```bash
# Example: Add migration for Users module
dotnet ef migrations add YourMigrationName \
    --project src/Modules/Users/Lanka.Modules.Users.Infrastructure
```

---

## Step 4: Access the Application

The API and Gateway are started automatically by Aspire (Step 2). No separate `dotnet run` needed.

### Access Points

| Service | URL | Notes |
|---------|-----|-------|
| Aspire Dashboard | *(shown in console output)* | Logs, traces, metrics |
| API | http://localhost:4307 | Main API |
| Health Check | http://localhost:4307/healthz | System status |
| Gateway | https://localhost:4308 | YARP reverse proxy |
| Kibana | http://localhost:5601 | Elasticsearch UI |
| Keycloak | http://localhost:18080/admin | Identity management (admin/admin) |
| RabbitMQ | http://localhost:15672 | Message queue admin |

---

## Step 5: Test an API Call

### Check Health

```bash
curl http://localhost:4307/healthz
# Should return: Healthy
```

### Register a User

```bash
curl -X POST http://localhost:4307/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

---

## Step 6: Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run architecture tests
cd test/Lanka.ArchitectureTests
dotnet test
```

---

## Daily Development Workflow

```bash
# 1. Pull latest changes
git pull origin main

# 2. Restore packages (if needed)
dotnet restore

# 3. Start everything with Aspire
dotnet run --project src/Api/Lanka.AppHost
```

---

## Troubleshooting

### Aspire Won't Start

```bash
# Verify Aspire workload is installed
dotnet workload list

# If missing, install it
dotnet workload install aspire

# Check Docker is running (Aspire needs Docker for containers)
docker info
```

### Database Connection Errors

```bash
# Check the Aspire Dashboard for container health status
# If volumes have stale credentials, reset them:
docker volume rm $(docker volume ls -q --filter name=lanka)
dotnet run --project src/Api/Lanka.AppHost
```

### Port Conflicts

```bash
# Find what's using a port
lsof -i :4307  # macOS/Linux
netstat -ano | findstr :4307  # Windows
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## What's Next?

- [Development Setup](development-setup.md) — Full environment configuration
- [Architecture Overview](../architecture/README.md) — Understand the system design
- [FAQ](faq.md) — Common questions and solutions

---

<div align="center">

*Happy coding!*

</div>
