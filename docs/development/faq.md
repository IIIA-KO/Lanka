# ❓ Lanka FAQ & Troubleshooting

<div align="center">

*Quick answers to common questions and solutions to frequent issues*


**"The only stupid question is the one not asked"**

</div>

---

## 🎯 **Quick Navigation**

<table>
<tr>
<td width="50%">

### **🚀 Getting Started**
- [Setup & Installation](#-setup--installation)
- [First Run Issues](#-first-run-issues)
- [Environment Configuration](#-environment-configuration)
- [Tool Setup](#-tool-setup)

### **🏗️ Development**
- [Building & Compilation](#-building--compilation)
- [Database & Migrations](#-database--migrations)
- [Testing Issues](#-testing-issues)
- [Debugging Problems](#-debugging-problems)

</td>
<td width="50%">

### **🐳 Infrastructure**
- [Docker & Containers](#-docker--containers)
- [Service Configuration](#-service-configuration)
- [Network & Connectivity](#-network--connectivity)
- [Performance Issues](#-performance-issues)

### **🔧 Advanced Topics**
- [Architecture Questions](#-architecture-questions)
- [Module Development](#-module-development)
- [Deployment Issues](#-deployment-issues)
- [Best Practices](#-best-practices)

</td>
</tr>
</table>

---

## 🚀 **Setup & Installation**

### **Q: What are the minimum system requirements?**
**A:** You need:
- **CPU**: 4+ cores (8+ recommended)
- **RAM**: 8 GB minimum (16+ GB recommended)
- **Storage**: 256 GB SSD minimum (512+ GB recommended)
- **OS**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 20.04+)

The high requirements are due to running multiple Docker containers for development.

### **Q: Can I develop on Apple Silicon (M1/M2) Macs?**
**A:** ✅ **Yes!** Lanka fully supports Apple Silicon:
```bash
# Use ARM64 images in docker-compose.yml
services:
  postgres:
    image: postgres:latest
    platform: linux/arm64
```

### **Q: Do I need Visual Studio or can I use VS Code?**
**A:** **Either works great!**
- **VS Code**: Lightweight, excellent extensions, free
- **Visual Studio**: Full IDE experience, advanced debugging
- **JetBrains Rider**: Cross-platform, powerful refactoring

All are fully supported with our development setup.

### **Q: Can I develop without Docker?**
**A:** **Not recommended.** While technically possible, Docker provides:
- ✅ **Consistent environment** across all developer machines
- ✅ **Easy service management** (databases, message queues)
- ✅ **Isolated dependencies** that don't conflict with your system
- ✅ **Production-like environment** for testing

---

## 🔥 **First Run Issues**

### **Q: `dotnet ef` command not found**
**A:** Install EF Core tools:
```bash
# Install globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### **Q: Migration fails with "framework name" error**
**A:** Run migrations from the module directory:
```bash
# ❌ Wrong (from solution root)
dotnet ef migrations add Test --project src/Modules/Analytics/Lanka.Modules.Analytics.Infrastructure

# ✅ Correct (from module directory)
cd src/Modules/Analytics/Lanka.Modules.Analytics.Infrastructure
dotnet ef migrations add Test
```

### **Q: Docker services won't start**
**A:** Common solutions:
```bash
# 1. Check Docker is running
docker info

# 2. Reset Docker services
docker compose down -v
docker compose up -d --force-recreate

# 3. Check port conflicts
netstat -an | grep :5432  # Check if PostgreSQL port is used

# 4. Free up Docker resources
docker system prune -a
```

### **Q: API returns 500 errors on startup**
**A:** Check these common issues:
1. **Database not ready**: Wait 30 seconds after `docker compose up`
2. **Missing migrations**: Run `dotnet ef database update` for each module
3. **Configuration errors**: Check `appsettings.json` connection strings
4. **Service dependencies**: Ensure PostgreSQL, Redis, RabbitMQ are healthy

---

## 🏗️ **Building & Compilation**

### **Q: Build fails with "package not found" errors**
**A:** Restore NuGet packages:
```bash
# Clean and restore
dotnet clean
dotnet restore

# If using centralized package management
dotnet restore --force

# Check for package conflicts
dotnet list package --include-transitive
```

### **Q: Getting "duplicate reference" warnings**
**A:** We use Central Package Management. Check `Directory.Packages.props`:
```xml
<!-- Don't specify versions in project files -->
<PackageReference Include="Newtonsoft.Json" />

<!-- Versions are managed centrally -->
<PackageVersion Include="Newtonsoft.Json" Version="13.0.3"/>
```

### **Q: Hot reload not working**
**A:** Use the correct command:
```bash
# For API development with hot reload
cd src/Api/Lanka.Api
dotnet watch run

# For specific project
dotnet watch --project src/Api/Lanka.Api run
```

### **Q: StyleCop/analyzer errors are overwhelming**
**A:** Configure analysis level in `Directory.Build.props`:
```xml
<PropertyGroup>
  <!-- Reduce analysis level during development -->
  <AnalysisLevel>5.0</AnalysisLevel>
  <!-- Or disable specific rules -->
  <NoWarn>CA1062;CA1031</NoWarn>
</PropertyGroup>
```

---

## 🗃️ **Database & Migrations**

### **Q: "DbContext cannot be created" during migrations**
**A:** Each module needs `Microsoft.EntityFrameworkCore.Design`:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

### **Q: How do I reset the database completely?**
**A:** Full database reset:
```bash
# 1. Stop API
pkill -f "Lanka.Api"

# 2. Reset PostgreSQL container
docker compose stop postgres
docker volume rm lanka_postgres_data
docker compose up -d postgres

# 3. Wait for PostgreSQL to start
sleep 30

# 4. Apply migrations
cd src/Modules/Users/Lanka.Modules.Users.Infrastructure
dotnet ef database update
# Repeat for Analytics and Campaigns modules
```

### **Q: Migration creates empty Up/Down methods**
**A:** This usually means:
1. **No model changes**: EF detected no differences
2. **Wrong DbContext**: Make sure you're in the right module
3. **Missing DbSet**: Entity not configured in DbContext

Check your entity is registered:
```csharp
public DbSet<YourEntity> YourEntities { get; set; }
```

### **Q: "Schema already exists" error**
**A:** Multiple modules use the same database with different schemas:
- **Users**: `users` schema
- **Analytics**: `analytics` schema  
- **Campaigns**: `campaigns` schema

This is by design for the modular monolith architecture.

---

## 🧪 **Testing Issues**

### **Q: Integration tests fail with database errors**
**A:** Integration tests use TestContainers:
```bash
# Make sure Docker is running
docker info

# Run tests with verbose output
dotnet test --logger:console;verbosity=detailed

# Run specific test category
dotnet test --filter Category=Integration
```

### **Q: Tests are slow**
**A:** Optimize test performance:
```csharp
// Use test fixtures for expensive setup
public class DatabaseFixture : IAsyncLifetime
{
    // Shared database container for all tests
}

// Parallel test execution
[assembly: CollectionBehavior(DisableTestParallelization = false)]
```

### **Q: "Port already in use" in tests**
**A:** TestContainers automatically assigns ports:
```csharp
// Let TestContainers choose ports
var container = new PostgreSqlBuilder()
    .WithDatabase("test_db")
    .Build(); // No .WithPortBinding()
```

---

## 🐳 **Docker & Containers**

### **Q: Containers keep restarting**
**A:** Check container logs:
```bash
# View logs for specific service
docker compose logs postgres

# Follow logs in real-time
docker compose logs -f rabbitmq

# Check container health
docker compose ps
```

### **Q: "Port already allocated" error**
**A:** Find and stop conflicting services:
```bash
# Check what's using the port
lsof -i :5432  # macOS/Linux
netstat -ano | findstr :5432  # Windows

# Stop specific service
docker stop lanka-postgres

# Change port in docker-compose.yml if needed
ports:
  - "5433:5432"  # Use different host port
```

### **Q: Docker containers use too much disk space**
**A:** Clean up Docker resources:
```bash
# See disk usage
docker system df

# Clean up unused resources
docker system prune -a

# Remove unused volumes
docker volume prune

# Remove Lanka-specific volumes
docker compose down -v
```

### **Q: Services are slow to start**
**A:** Optimize Docker performance:
```yaml
# Add health checks for dependencies
services:
  api:
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
```

---

## 🔧 **Service Configuration**

### **Q: Keycloak admin console not accessible**
**A:** Check Keycloak startup:
```bash
# View Keycloak logs
docker compose logs keycloak

# Keycloak takes time to start - wait 2-3 minutes
# Then access: http://localhost:18080/admin
# Username: admin, Password: admin
```

### **Q: RabbitMQ management UI shows "Not found"**
**A:** Ensure using the management image:
```yaml
rabbitmq:
  image: rabbitmq:3-management-alpine  # Note: -management
  ports:
    - "15672:15672"  # Management UI port
```

### **Q: Can't connect to MongoDB**
**A:** Check MongoDB configuration:
```bash
# Test connection
mongosh "mongodb://localhost:27017/lanka_analytics"

# Check if MongoDB container is running
docker compose ps mongodb

# View MongoDB logs
docker compose logs mongodb
```

---

## 🌐 **Network & Connectivity**

### **Q: CORS errors in frontend**
**A:** Configure CORS in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors();
```

### **Q: Health checks fail**
**A:** Check health check configuration:
```bash
# Test health endpoint
curl http://localhost:4307/healthz

# Should return JSON with service statuses
```

---

## 📊 **Performance Issues**

### **Q: API responses are slow**
**A:** Common optimizations:

1. **Use Redis caching**:
```csharp
await _cache.SetStringAsync(key, value, TimeSpan.FromMinutes(5));
```

2. **Optimize database queries**:
```csharp
// Use projections instead of full entities
var result = await context.Users
    .Select(u => new { u.Id, u.Email })
    .ToListAsync();
```

### **Q: High memory usage**
**A:** Monitor and optimize:
```bash
# Check Docker memory usage
docker stats

# Profile .NET memory usage
dotnet-counters monitor --process-id <PID>

# Use memory profiling tools
dotMemoryUnit
```

### **Q: Database queries are slow**
**A:** Database optimization:
```sql
-- Check query performance
EXPLAIN ANALYZE SELECT * FROM users WHERE email = 'test@example.com';

-- Add indexes for common queries
CREATE INDEX idx_users_email ON users(email);
```

---

## 🏗️ **Architecture Questions**

### **Q: How do modules communicate?**
**A:** Modules use **event-driven communication**:
```csharp
// Publish domain event
await _publisher.Publish(new UserRegisteredEvent(userId));

// Handle in another module
public class UserRegisteredHandler : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Create analytics profile
    }
}
```

### **Q: Can I access one module's data from another?**
**A:** **No direct access.** Use integration events:
- ✅ **Events**: `UserRegisteredEvent`, `CampaignCreatedEvent`
- ❌ **Direct DB access**: Don't query other module's tables
- ❌ **Direct references**: Don't reference other module's entities

### **Q: Where do I put shared code?**
**A:** Use the **Common projects**:
- `Lanka.Common.Domain` - Shared domain primitives
- `Lanka.Common.Application` - Shared application patterns
- `Lanka.Common.Infrastructure` - Shared infrastructure

### **Q: How do I add a new module?**
**A:** Follow the module template:
```
src/Modules/YourModule/
├── Lanka.Modules.YourModule.Domain/
├── Lanka.Modules.YourModule.Application/
├── Lanka.Modules.YourModule.Infrastructure/
└── Lanka.Modules.YourModule.Presentation/
```

---

## 🔄 **Module Development**

### **Q: How do I create a new aggregate?**
**A:** Follow DDD patterns:
```csharp
public class YourAggregate : Entity<YourAggregateId>
{
    public static Result<YourAggregate> Create(/* parameters */)
    {
        // Validation logic
        var aggregate = new YourAggregate(/* ... */);
        
        // Raise domain event
        aggregate.RaiseDomainEvent(new YourAggregateCreatedEvent(/*...*/));
        
        return aggregate;
    }
}
```

### **Q: How do I add a new command/query?**
**A:** Use CQRS pattern:
```csharp
// Command
public record CreateUserCommand(string Email, string Password) : IRequest<Result<Guid>>;

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### **Q: How do I add validation?**
**A:** Use FluentValidation:
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
```

---

## 🚀 **Deployment Issues**

### **Q: Docker build fails in CI/CD**
**A:** Common solutions:
```dockerfile
# Use multi-stage builds
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.sln .
COPY Directory.Packages.props .
# Copy all project files first
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore

# Then copy source and build
COPY src/ .
RUN dotnet build
```

### **Q: Environment variables not loading**
**A:** Check configuration priority:
1. Environment variables (highest)
2. appsettings.Environment.json
3. appsettings.json (lowest)

```csharp
// In Program.cs
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment}.json", optional: true)
    .AddEnvironmentVariables();
```

---

## 💡 **Best Practices**

### **Q: What's the recommended Git workflow?**
**A:** Use feature branches:
```bash
# Create feature branch
git checkout -b feature/user-authentication

# Make commits with conventional format
git commit -m "feat: add user registration endpoint"
git commit -m "fix: resolve validation issue"

# Push and create PR
git push origin feature/user-authentication
```

### **Q: How should I structure my tests?**
**A:** Follow the test pyramid:
```
     Unit Tests (70%)
    /              \
   Integration Tests (20%)
  /                      \
 -- End-2-Enf Tests (10%) --
```

### **Q: What naming conventions should I use?**
**A:** Follow C# conventions:
- **Classes**: `PascalCase` (e.g., `UserService`)
- **Methods**: `PascalCase` (e.g., `GetUserById`)
- **Properties**: `PascalCase` (e.g., `FirstName`)
- **Fields**: `_camelCase` (e.g., `_userRepository`)
- **Constants**: `PascalCase` (e.g., `MaxRetryCount`)

---

## 🆘 **Getting Help**

### **🔍 Diagnostic Commands**
```bash
# Check system status
./scripts/health-check.sh

# View all logs
docker compose logs

# Check API health
curl http://localhost:4307/healthz

# Test database connection
psql -h localhost -U postgres -d lanka_dev -c "SELECT 1;"
```

### **📞 Where to Get Support**
- **🐛 Bugs**: Open GitHub issue with reproduction steps
- **❓ Questions**: Check this FAQ first, then ask the team
- **💡 Feature Requests**: Discuss in team meetings
- **🔧 Environment Issues**: Use the diagnostic commands above

### **📝 Reporting Issues**
When reporting issues, include:
1. **Steps to reproduce**
2. **Expected vs actual behavior**
3. **Environment info** (`dotnet --version`, OS, Docker version)
4. **Logs** (API logs, Docker logs, etc.)
5. **Screenshots** if relevant

---

<div align="center">

**Still need help? 🤝**

[![GitHub Issues](https://img.shields.io/badge/🐛-Report%20Bug-red?style=for-the-badge)](https://github.com/IIIA-KO/Lanka/issues)
[![Discussions](https://img.shields.io/badge/💬-Ask%20Question-blue?style=for-the-badge)](https://github.com/IIIA-KO/Lanka/discussions)
[![Documentation](https://img.shields.io/badge/📚-Read%20Docs-green?style=for-the-badge)](../README.md)

</div>