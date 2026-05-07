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

### **1. .NET 10.0 SDK**
```bash
# Windows (via winget)
winget install Microsoft.DotNet.SDK.10

# macOS (via Homebrew)
brew install --cask dotnet-sdk

# Linux (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verify installation
dotnet --version
# Should output: 10.0.x
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

### **1. .NET Aspire Orchestration**

Lanka uses **.NET Aspire** to orchestrate all infrastructure containers and application projects. The orchestration is defined in C# code in the AppHost project.

**Prerequisites:**
```bash
# Install the Aspire workload (one-time)
dotnet workload install aspire
```

The AppHost (`src/Api/Lanka.AppHost/Program.cs`) defines all resources:

| Resource | Container | Fixed Port |
|----------|-----------|------------|
| PostgreSQL | `lanka-postgres` | 5432 |
| Redis | `lanka-cache` | 6379 |
| RabbitMQ | `lanka-queue` | 5672 |
| MongoDB | `lanka-mongo` | 27017 |
| Elasticsearch | `lanka-search` | *(dynamic)* |
| Keycloak | `lanka-identity` | 18080 |
| Kibana | `lanka-kibana` | 5601 |
| Lanka.Api | *(project)* | *(dynamic)* |
| Lanka.Gateway | *(project)* | 4308 |
| Lanka Client | *(JavaScript app)* | 4200 |
| WayForPay tunnel | `ngrok/ngrok` | *(enabled only when configured)* |

Aspire handles container lifecycle, health checks, connection string injection, and startup ordering (`WaitFor()`).

### **2. Start Infrastructure**

```bash
# Start everything (containers + API + Gateway)
dotnet run --project src/Api/Lanka.AppHost

# The Aspire Dashboard URL is printed to the console — use it for logs, traces, metrics

# Stop: Ctrl+C (stops all containers and projects)

# Reset all data (clean slate)
docker volume rm $(docker volume ls -q --filter name=lanka)
dotnet run --project src/Api/Lanka.AppHost
```

---

## 🗃️ **Database Setup**

### **1. Automatic Provisioning**

When running via Aspire, databases are created automatically by the AppHost. PostgreSQL gets a `lanka` database, MongoDB gets a `Mongo` database. No manual SQL scripts needed.

### **2. Database Tools Setup (Optional)**

For direct database access during development (e.g., via DBeaver, pgAdmin, or MongoDB Compass), use the connection details from the [Tools README](../tools/README.md#database-connections).

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

When running via Aspire, **connection strings are auto-injected** — the empty values in `appsettings.json` are overridden at runtime by the AppHost. The resource names in the AppHost map to connection string keys:

| AppHost Resource Name | Config Key |
|----------------------|------------|
| `"Database"` | `ConnectionStrings:Database` |
| `"Cache"` | `ConnectionStrings:Cache` |
| `"Queue"` | `ConnectionStrings:Queue` |
| `"Mongo"` | `ConnectionStrings:Mongo` |

Other settings:
- `Authentication` (Keycloak OIDC):
  - `Audience`: usually `account`
  - `TokenValidationParameters.ValidIssuers`: allowed issuers list
  - `MetadataAddress`: `http://localhost:18080/realms/lanka/.well-known/openid-configuration`
  - `RequireHttpsMetadata`: false for local dev
- `KeyCloak.HealthUrl`: Keycloak health endpoint
- `Serilog`: Console sink; structured logs are bridged to OpenTelemetry via `writeToProviders: true` and appear in the Aspire Dashboard

Passwords for infrastructure containers are configured in `src/Api/Lanka.AppHost/appsettings.Development.json` under the `Parameters` key.

### **2. WayForPay Development Checkout**

Lanka uses WayForPay hosted checkout for the demo payment flow. The checkout page can open from localhost, but WayForPay server callbacks must reach the local Gateway. For that, the AppHost can start an ngrok container automatically and route the public HTTPS URL to the local HTTPS Gateway at `https://localhost:4308`.

The AppHost reads two Aspire parameters:

| Parameter | Purpose |
|-----------|---------|
| `ngrok-auth-token` | Auth token for the ngrok container |
| `wayforpay-public-base-url` | Static public ngrok domain, for example `https://your-domain.ngrok-free.app` |

When both values are real values, AppHost:
- starts the `wayforpay-tunnel` container;
- forwards the public ngrok domain to `https://host.docker.internal:4308`;
- injects `Campaigns__WayForPay__PublicBaseUrl` into `Lanka.Api`;
- makes WayForPay `serviceUrl` and `returnUrl` use the public base URL instead of localhost.

For local development, set the values in `src/Api/Lanka.AppHost/appsettings.Development.json` or with user secrets:

```bash
dotnet user-secrets set "Parameters:ngrok-auth-token" "<your-ngrok-token>" --project src/Api/Lanka.AppHost
dotnet user-secrets set "Parameters:wayforpay-public-base-url" "https://your-domain.ngrok-free.app" --project src/Api/Lanka.AppHost
```

If either value is missing or still set to the placeholder, AppHost does not start the tunnel and the API falls back to the URLs from `src/Api/Lanka.Api/modules.campaigns*.json`.

The checked-in development WayForPay merchant is a test merchant. It is useful for a diploma/demo checkout page and provider callback testing, but it is not a marketplace payout implementation: money collection and creator payout reconciliation are not automated beyond the app's campaign/payment state.

### **3. Gateway appsettings.json**
Location: `src/Api/Lanka.Gateway/appsettings.json`

- `Authentication`: same as API
- `Serilog`: same approach as API
- `ReverseProxy`: routes/clusters configuration (to be filled per deployment; YARP is integrated).

### **4. Module configuration**
Each module configures its EF Core schema and applies migrations automatically. No per-module appsettings files are required for dev beyond the API/Gateway.

### **5. Keycloak Realm Import**
Keycloak starts with `--import-realm` and mounts `.files` into the container. Import the realm from `test/Lanka.IntegrationTests/lanka-realm-export.json`.

Access Keycloak Admin at `http://localhost:18080/admin` (admin/admin) and verify realm `lanka` exists.

### **6. Instagram Integration (Development Policy)**
Use the repository owner's Meta app settings for development, or create your own Meta Business app and configure Instagram Graph API permissions if necessary. This process is complex; external contributors are expected to focus on feature development and reuse provided development configuration.

### **7. Launch Profiles**
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
      "program": "${workspaceFolder}/src/Api/Lanka.Api/bin/Debug/net10.0/Lanka.Api.dll",
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

# 1. Pull latest changes
echo "📥 Pulling latest changes..."
git pull origin main

# 2. Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# 3. Start everything with Aspire (containers + API + Gateway)
echo "🚀 Starting Aspire AppHost..."
dotnet run --project src/Api/Lanka.AppHost

# Aspire handles:
# - Starting all infrastructure containers
# - Waiting for health checks
# - Launching Lanka.Api and Lanka.Gateway
# - Opening the Aspire Dashboard for logs/traces/metrics
```

### **🛑 Daily Shutdown Routine**
```bash
# Press Ctrl+C in the terminal running the AppHost
# Aspire stops all containers and projects automatically
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
# Check Docker resources
docker system df
docker stats

# Reset all Lanka data volumes
docker volume rm $(docker volume ls -q --filter name=lanka)
```

**Database Connection Issues**
```bash
# Test PostgreSQL connection
psql -h localhost -U postgres -c "SELECT version();"

# Check container logs via the Aspire Dashboard (URL in console output)

# Reset database volume and restart
docker volume rm $(docker volume ls -q --filter name=lanka-postgres)
dotnet run --project src/Api/Lanka.AppHost
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

# Check API health (returns JSON with all dependency statuses)
echo -n "🌐 API Health: "
curl -s http://localhost:4307/healthz | head -c 200 || echo "❌ API not responding"

# Tip: Open the Aspire Dashboard (URL in console output) for full resource status

echo ""
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
