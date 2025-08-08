# 🛠️ Lanka Development Setup Guide

<div align="center">

*Complete environment setup for productive Lanka development*

![Development Setup](../images/dev-setup-banner.png)

</div>

---

## 🎯 **Development Environment Overview**

Lanka development environment is designed to be **consistent**, **reproducible**, and **developer-friendly**. We use containerized infrastructure with hot-reload capabilities for a seamless development experience.

---

## 💻 **Workstation Requirements**

### **🖥️ Hardware Recommendations**
| Component | Minimum | Recommended | Why? |
|-----------|---------|-------------|------|
| **CPU** | 4 cores | 8+ cores | Docker containers, compilation |
| **RAM** | 8 GB | 16+ GB | Multiple services, caching |
| **Storage** | 256 GB SSD | 512+ GB SSD | Docker images, dependencies |
| **Network** | Broadband | High-speed | Package downloads, updates |

### **🌐 Operating System Support**
- ✅ **Windows 10/11** - Full Docker Desktop support
- ✅ **macOS** - Intel and Apple Silicon
- ✅ **Linux** - Ubuntu 20.04+, Docker native support

---

## 🔧 **Core Tools Installation**

### **1. .NET 9.0 SDK**
```bash
# Windows (via winget)
winget install Microsoft.DotNet.SDK.9

# macOS (via Homebrew)
brew install --cask dotnet-sdk

# Linux (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# Verify installation
dotnet --version
# Should output: 9.0.x
```

### **2. Docker Desktop**
```bash
# Windows
# Download from: https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe

# macOS
brew install --cask docker

# Linux (Docker Engine)
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
newgrp docker

# Verify installation
docker --version
docker compose version
```

### **3. Git Configuration**
```bash
# Install Git (if not already installed)
# Windows: git-scm.com
# macOS: brew install git
# Linux: sudo apt-get install git

# Configure user identity
git config --global user.name "Your Name"
git config --global user.email "your.email@company.com"

# Configure line endings
git config --global core.autocrlf true    # Windows
git config --global core.autocrlf input   # macOS/Linux

# Configure default branch
git config --global init.defaultBranch main

# Enable credential helper
git config --global credential.helper store
```

---

## 🎨 **IDE Setup**

### **🎯 Visual Studio Code**

#### **Essential Extensions**
```bash
# Install VS Code extensions via command line
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.vscode-dotnet-runtime
code --install-extension ms-vscode-remote.remote-containers
code --install-extension ms-azuretools.vscode-docker
code --install-extension humao.rest-client
code --install-extension bradlc.vscode-tailwindcss
code --install-extension esbenp.prettier-vscode
code --install-extension redhat.vscode-yaml
```

#### **Workspace Settings**
```json
{
  "dotnet.defaultSolution": "Lanka.sln",
  "omnisharp.enableRoslynAnalyzers": true,
  "csharp.semanticHighlighting.enabled": true,
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true,
    "source.organizeImports": true
  },
  "files.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/.vs": true
  }
}
```

### **🔥 JetBrains Rider (Alternative)**

#### **Essential Plugins**
- **Docker** - Container management
- **Database Tools** - PostgreSQL/MongoDB support
- **GitToolBox** - Enhanced Git integration
- **HTTP Client** - API testing
- **Sequence Diagram** - Code flow visualization

#### **Rider Configuration**
```xml
<!-- Rider settings for optimal Lanka development -->
<application>
  <component name="EditorSettings">
    <option name="USE_SOFT_WRAPS" value="true" />
    <option name="STRIP_TRAILING_SPACES" value="CHANGED" />
  </component>
  <component name="CodeStyleSettings">
    <option name="RIGHT_MARGIN" value="120" />
    <option name="WRAP_ON_TYPING" value="1" />
  </component>
</application>
```

---

## 🐳 **Container Infrastructure Setup**

### **1. Infrastructure Services**
```yaml
# docker-compose.yml - Core infrastructure
version: '3.8'

services:
  # Primary Database
  postgres:
    image: postgres:latest
    container_name: lanka-postgres
    environment:
      POSTGRES_DB: lanka_dev
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Analytics Database
  mongodb:
    image: mongo:latest
    container_name: lanka-mongodb
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Caching Layer
  redis:
    image: redis:latest
    container_name: lanka-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Message Bus
  rabbitmq:
    image: rabbitmq:management-alpine
    container_name: lanka-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: lanka
      RABBITMQ_DEFAULT_PASS: development
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Identity Provider
  keycloak:
    image: quay.io/keycloak/keycloak:latest
    container_name: lanka-keycloak
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    ports:
      - "18080:8080"
    command: start-dev

  # Log Aggregation
  seq:
    image: datalust/seq:latest
    container_name: lanka-seq
    environment:
      ACCEPT_EULA: Y
    ports:
      - "8081:80"

volumes:
  postgres_data:
  mongodb_data:
  redis_data:
  rabbitmq_data:
  seq_data:

networks:
  default:
    name: lanka-network
    driver: bridge
```

### **2. Start Infrastructure**

```bash
# Start all services
docker compose up -d

# Check service health
docker compose ps

# View logs
docker compose logs -f [service-name]

# Stop all services
docker compose down

# Reset all data (clean slate)
docker compose down -v
docker compose up -d
```

---

## 🗃️ **Database Setup**

### **1. PostgreSQL Configuration**
```sql
-- scripts/init-db.sql - Initial database setup
CREATE DATABASE lanka_dev;
CREATE DATABASE lanka_test;
CREATE DATABASE keycloak;

-- Create users for different environments
CREATE USER lanka_app WITH PASSWORD 'lanka_development';
GRANT ALL PRIVILEGES ON DATABASE lanka_dev TO lanka_app;
GRANT ALL PRIVILEGES ON DATABASE lanka_test TO lanka_app;
```

### **2. Database Tools Setup**
```bash
# Install PostgreSQL client tools
# Windows
winget install PostgreSQL.PostgreSQL

# macOS
brew install postgresql

# Linux
sudo apt-get install postgresql-client

# Test connection
psql -h localhost -U postgres -d lanka_dev
```

### **3. Migrations Policy**
Migrations are applied automatically per module when the API starts. Use the CLI only to generate new migrations during development:
```bash
# Example: generate a migration for Users module
dotnet ef migrations add <Name> --project src/Modules/Users/Lanka.Modules.Users.Infrastructure
```

---

## 🔧 **Application Configuration**

### **1. API appsettings.json**
Location: `src/Api/Lanka.Api/appsettings.json`

- `ConnectionStrings.Database`: PostgreSQL connection string (e.g., `Host=localhost;Port=5432;Database=lanka;Username=postgres;Password=postgres`)
- `ConnectionStrings.Cache`: Redis connection (e.g., `localhost:6379`)
- `ConnectionStrings.Queue`: RabbitMQ connection (e.g., `amqp://guest:guest@localhost:5672`)
- `ConnectionStrings.Mongo`: MongoDB connection (e.g., `mongodb://admin:admin@localhost:27017`)
- `Authentication` (Keycloak OIDC):
  - `Audience`: usually `account`
  - `TokenValidationParameters.ValidIssuers`: allowed issuers list
  - `MetadataAddress`: `http://localhost:18080/realms/lanka/.well-known/openid-configuration`
  - `RequireHttpsMetadata`: false for local dev
- `KeyCloak.HealthUrl`: Keycloak health endpoint inside Docker network (e.g., `http://lanka.identity:8080/health/ready`)
- `Serilog` → Seq sink uses `http://lanka.seq:5341` (container address/port). Seq UI is at `http://localhost:8081` on host.
- `OTEL_EXPORTER_OTLP_ENDPOINT`: optional OpenTelemetry exporter endpoint.

### **2. Gateway appsettings.json**
Location: `src/Api/Lanka.Gateway/appsettings.json`

- `Kestrel.Endpoints.Https.Url`: `https://+:4308`
- `Authentication`: same as API
- `Serilog`: same approach as API
- `ReverseProxy`: routes/clusters configuration (to be filled per deployment; YARP is integrated).

### **3. Module configuration**
Each module configures its EF Core schema and applies migrations automatically. No per-module appsettings files are required for dev beyond the API/Gateway.

### **4. Keycloak Realm Import**
Keycloak starts with `--import-realm` and mounts `.files` into the container. Import the realm from `test/Lanka.IntegrationTests/lanka-realm-export.json`.

Access Keycloak Admin at `http://localhost:18080/admin` (admin/admin) and verify realm `lanka` exists.

### **5. Instagram Integration (Development Policy)**
Use the repository owner's Meta app settings for development, or create your own Meta Business app and configure Instagram Graph API permissions if necessary. This process is complex; external contributors are expected to focus on feature development and reuse provided development configuration.

### **2. Launch Profiles**
```json
{
  "profiles": {
    "Lanka.Api": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "http://localhost:4307"
    },
    "Lanka.Api (Docker)": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
      "environmentVariables": {
        "ASPNETCORE_URLS": "http://+:4307"
      },
      "publishAllPorts": true,
      "useSSL": true
    }
  }
}
```

---

## 🧪 **Testing Environment Setup**

### **1. Test Database Configuration**
```bash
# Test-specific environment variables
export TESTING_DATABASE_CONNECTION_STRING="Host=localhost;Database=lanka_test;Username=postgres;Password=postgres"
export TESTING_REDIS_CONNECTION_STRING="localhost:6379"

# Run integration tests with TestContainers
cd test/Lanka.IntegrationTests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"
```

### **2. Performance Testing Setup**
```bash
# Install NBomber for load testing
dotnet add package NBomber

# Run performance tests
cd test/Lanka.PerformanceTests
dotnet run --configuration Release
```

---

## 🔍 **Debugging Configuration**

### **1. VS Code Debug Configuration**
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Lanka.Api",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Api/Lanka.Api/bin/Debug/net9.0/Lanka.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Api/Lanka.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    },
    {
      "name": "Attach to Lanka.Api",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickProcess}"
    }
  ]
}
```

### **2. Remote Debugging**
```json
{
  "name": "Remote Debug",
  "type": "coreclr",
  "request": "attach",
  "processId": "${command:pickRemoteProcess}",
  "pipeTransport": {
    "pipeCwd": "${workspaceFolder}",
    "pipeProgram": "docker",
    "pipeArgs": ["exec", "-i", "lanka-api"],
    "debuggerPath": "/vsdbg/vsdbg",
    "quoteArgs": false
  }
}
```

---

## 🚀 **Development Workflow**

### **📅 Daily Startup Routine**
```bash
#!/bin/bash
# scripts/dev-start.sh - Daily development startup

echo "🚀 Starting Lanka development environment..."

# 1. Start infrastructure services
echo "📦 Starting infrastructure..."
docker-compose up -d

# 2. Wait for services to be healthy
echo "⏳ Waiting for services..."
sleep 30

# 3. Check service health
echo "💚 Checking service health..."
docker compose ps

# 4. Pull latest changes
echo "📥 Pulling latest changes..."
git pull origin main

# 5. Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# 6. Build solution
echo "🔨 Building solution..."
dotnet build

# 7. Run migrations if needed
echo "🗃️ Updating databases..."
cd src/Modules/Users/Lanka.Modules.Users.Infrastructure && dotnet ef database update
cd ../../../Analytics/Lanka.Modules.Analytics.Infrastructure && dotnet ef database update
cd ../../../Campaigns/Lanka.Modules.Campaigns.Infrastructure && dotnet ef database update
cd ../../../../..

echo "✅ Development environment ready!"
echo "🌐 API: http://localhost:4307"
echo "💚 Health: http://localhost:4307/healthz"
```

### **🛑 Daily Shutdown Routine**
```bash
#!/bin/bash
# scripts/dev-stop.sh - Clean shutdown

echo "🛑 Stopping Lanka development environment..."

# 1. Stop running applications
echo "📱 Stopping applications..."
pkill -f "Lanka.Api"

# 2. Stop infrastructure services
echo "📦 Stopping infrastructure..."
docker-compose stop

echo "✅ Environment stopped cleanly!"
```

---

## 🔧 **Troubleshooting**

### **🔴 Common Issues**

**Port Conflicts**
```bash
# Find what's using a port
netstat -ano | findstr :4307    # Windows
lsof -i :4307                   # macOS/Linux

# Kill process using port
taskkill /PID <PID> /F          # Windows
kill -9 <PID>                   # macOS/Linux
```

**Docker Issues**
```bash
# Reset Docker
docker system prune -a
docker compose down -v
docker compose up -d --force-recreate

# Check Docker resources
docker system df
docker stats
```

**Database Connection Issues**
```bash
# Test PostgreSQL connection
psql -h localhost -U postgres -c "SELECT version();"

# Check Docker logs
docker compose logs postgres

# Reset database
docker compose stop postgres
docker volume rm lanka_postgres_data
docker compose up -d postgres
```

**Build Issues**
```bash
# Clean and restore
dotnet clean
rm -rf bin/ obj/
dotnet restore
dotnet build
```

---

## 📊 **Environment Health Check**

### **🏥 Health Check Script**
```bash
#!/bin/bash
# scripts/health-check.sh

echo "🏥 Lanka Environment Health Check"
echo "================================"

# Check .NET SDK
echo -n "✅ .NET SDK: "
dotnet --version

# Check Docker
echo -n "✅ Docker: "
docker --version

# Check running services
echo "📦 Infrastructure Services:"
docker compose ps

# Check API health
echo -n "🌐 API Health: "
curl -s http://localhost:4307/healthz || echo "❌ API not responding"

# Check database connectivity
echo -n "🗃️ Database: "
psql -h localhost -U postgres -d lanka_dev -c "SELECT 1;" > /dev/null 2>&1 && echo "✅ Connected" || echo "❌ Connection failed"

echo "================================"
echo "🎉 Health check complete!"
```

---

<div align="center">

**Your development environment is now ready! 🎉**

[![Start Development](https://img.shields.io/badge/🚀-Start%20Development-green?style=for-the-badge)](quick-start.md)
[![View Architecture](https://img.shields.io/badge/🏗️-View%20Architecture-blue?style=for-the-badge)](../architecture/README.md)
[![FAQ](https://img.shields.io/badge/❓-FAQ-orange?style=for-the-badge)](faq.md)

</div>