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

## Step 2: Start Infrastructure

Lanka uses Docker Compose for local infrastructure.

```bash
# Start all services
docker compose up -d

# Check they're running
docker compose ps
```

You should see these services:
- **PostgreSQL** (5432) — Primary database
- **MongoDB** (27017) — Analytics storage
- **Redis** (6379) — Caching
- **RabbitMQ** (5672, Management: 15672) — Message bus
- **Keycloak** (18080) — Identity provider
- **Seq** (8081) — Centralized logging
- **Elasticsearch** (9200) — Search index
- **Jaeger** (16686) — Distributed tracing

### Verify Services

```bash
curl http://localhost:15672  # RabbitMQ Management (guest/guest)
curl http://localhost:18080  # Keycloak (admin/admin)
curl http://localhost:8081   # Seq logs
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

## Step 4: Run the Application

```bash
# Start the API
cd src/Api/Lanka.Api
dotnet run

# Or with hot reload
dotnet watch run
```

### Access Points

| Service | URL | Notes |
|---------|-----|-------|
| API | http://localhost:4307 | Main API |
| Health Check | http://localhost:4307/healthz | System status |
| Gateway | http://localhost:4308 | YARP reverse proxy |
| Seq Logs | http://localhost:8081 | View structured logs |

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
# 1. Start infrastructure (if not running)
docker compose up -d

# 2. Pull latest changes
git pull origin main

# 3. Restore packages (if needed)
dotnet restore

# 4. Run API with hot reload
cd src/Api/Lanka.Api
dotnet watch run
```

---

## Troubleshooting

### Docker Services Won't Start

```bash
# Check Docker is running
docker info

# Reset services
docker compose down -v
docker compose up -d --force-recreate
```

### Database Connection Errors

```bash
# Check PostgreSQL is running
docker compose ps postgres

# View logs
docker compose logs postgres
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
