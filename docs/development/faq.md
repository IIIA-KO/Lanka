# FAQ & Troubleshooting

<div align="center">

*Common questions and solutions*

</div>

---

## Quick Navigation

<table>
<tr>
<td width="50%">

### Getting Started

- [Setup & Installation](#setup--installation)
- [First Run Issues](#first-run-issues)
- [Environment Configuration](#environment-configuration)

### Development

- [Building & Compilation](#building--compilation)
- [Database & Migrations](#database--migrations)
- [Testing Issues](#testing-issues)

</td>
<td width="50%">

### Infrastructure

- [Docker & Containers](#docker--containers)
- [Service Configuration](#service-configuration)
- [Performance Issues](#performance-issues)

### Advanced

- [Architecture Questions](#architecture-questions)
- [Module Development](#module-development)
- [Best Practices](#best-practices)

</td>
</tr>
</table>

---

## Setup & Installation

### What are the minimum system requirements?

- **CPU**: 4+ cores (8+ recommended)
- **RAM**: 8 GB minimum (16+ GB recommended)
- **Storage**: 256 GB SSD
- **OS**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 20.04+)

The high requirements are due to running multiple Docker containers.

### Can I develop on Apple Silicon (M1/M2/M3) Macs?

Yes. Docker images work on ARM64. The docker-compose.yml handles this automatically.

### Do I need Visual Studio or can I use VS Code?

Either works. JetBrains Rider is also fully supported. Use whatever you're comfortable with.

### Can I develop without Docker?

Not recommended. You'd need to install and configure PostgreSQL, MongoDB, Redis, RabbitMQ, Keycloak, and Elasticsearch locally. Docker makes this much simpler.

---

## First Run Issues

### `dotnet ef` command not found

Install EF Core tools globally:

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

### Migration fails with framework name error

Run migrations from the module's Infrastructure directory:

```bash
# Correct
cd src/Modules/Users/Lanka.Modules.Users.Infrastructure
dotnet ef migrations add Test

# Wrong - from solution root
dotnet ef migrations add Test --project src/Modules/Users/Lanka.Modules.Users.Infrastructure
```

### Docker services won't start

```bash
# Check Docker is running
docker info

# Reset services
docker compose down -v
docker compose up -d --force-recreate

# Check for port conflicts
lsof -i :5432  # PostgreSQL
lsof -i :4307  # API
```

### API returns 500 errors on startup

1. **Database not ready**: Wait 30 seconds after `docker compose up`
2. **Migrations not applied**: They run automatically, but check the logs
3. **Configuration errors**: Verify `appsettings.json` connection strings
4. **Service dependencies**: Ensure PostgreSQL, Redis, RabbitMQ are healthy

---

## Building & Compilation

### Build fails with "package not found"

```bash
dotnet clean
dotnet restore --force
dotnet build
```

### Getting duplicate reference warnings

We use Central Package Management. Don't specify versions in project files:

```xml
<!-- Don't do this -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />

<!-- Do this -->
<PackageReference Include="Newtonsoft.Json" />
```

Versions are managed in `Directory.Packages.props`.

### Hot reload not working

```bash
cd src/Api/Lanka.Api
dotnet watch run
```

Make sure you're using `watch` not `run`.

---

## Database & Migrations

### "DbContext cannot be created" during migrations

Each module needs `Microsoft.EntityFrameworkCore.Design`:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### How do I reset the database completely?

```bash
# Stop the API
docker compose stop postgres
docker volume rm lanka_postgres_data
docker compose up -d postgres

# Wait 30 seconds, then restart API (migrations run automatically)
```

### Migration creates empty Up/Down methods

This means EF detected no model changes. Check:
1. Entity is registered in DbContext
2. You're in the right module directory
3. Properties have changes that EF tracks

### "Schema already exists" error

Each module uses its own schema (`users`, `analytics`, `campaigns`). This is expected behavior for the modular monolith pattern.

---

## Testing Issues

### Integration tests fail with database errors

Integration tests use TestContainers. Make sure Docker is running:

```bash
docker info
dotnet test --logger:console;verbosity=detailed
```

### Tests are slow

Use test fixtures to share expensive setup (like database containers) across tests.

### "Port already in use" in tests

TestContainers automatically assigns random ports. Don't manually specify ports in test configuration.

---

## Docker & Containers

### Containers keep restarting

```bash
# Check logs
docker compose logs postgres
docker compose logs keycloak

# Check health status
docker compose ps
```

### "Port already allocated" error

```bash
# Find what's using the port
lsof -i :5432  # macOS/Linux
netstat -ano | findstr :5432  # Windows

# Stop the conflicting service or change ports in docker-compose.yml
```

### Docker uses too much disk space

```bash
docker system df
docker system prune -a
docker volume prune
```

---

## Service Configuration

### Keycloak admin console not accessible

Keycloak takes 2-3 minutes to start. Check logs:

```bash
docker compose logs keycloak
```

Then access: http://localhost:18080/admin (admin/admin)

### RabbitMQ management UI not working

Make sure you're using the management image:

```yaml
rabbitmq:
  image: rabbitmq:3-management-alpine  # Note: -management
```

Access: http://localhost:15672 (guest/guest)

### Can't connect to MongoDB

```bash
# Test connection
mongosh "mongodb://localhost:27017/lanka_analytics"

# Check container
docker compose logs mongodb
```

---

## Performance Issues

### API responses are slow

1. Use Redis caching for frequently accessed data
2. Optimize database queries with projections
3. Check for N+1 query problems in EF

### High memory usage

```bash
# Check Docker resource usage
docker stats

# .NET memory diagnostics
dotnet-counters monitor --process-id <PID>
```

### Database queries are slow

```sql
EXPLAIN ANALYZE SELECT * FROM users WHERE email = 'test@example.com';
```

Add indexes for frequently queried columns.

---

## Architecture Questions

### How do modules communicate?

Through **integration events** via RabbitMQ. Not direct method calls or database queries.

```csharp
// Publisher
await _eventBus.PublishAsync(new UserCreatedIntegrationEvent(...));

// Handler in another module
public class UserCreatedHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public Task Handle(UserCreatedIntegrationEvent @event) { ... }
}
```

### Can I access one module's data from another?

No. Use events for communication. Each module owns its data.

### Where do I put shared code?

In the Common projects:
- `Lanka.Common.Domain` — Base entities, value objects
- `Lanka.Common.Application` — Base handlers, behaviors
- `Lanka.Common.Infrastructure` — Shared infrastructure

### How do I add a new module?

Create the folder structure under `src/Modules/YourModule/`:
- `Lanka.Modules.YourModule.Domain`
- `Lanka.Modules.YourModule.Application`
- `Lanka.Modules.YourModule.Infrastructure`
- `Lanka.Modules.YourModule.Presentation`

Follow the Users module as a reference.

---

## Module Development

### How do I create a new aggregate?

```csharp
public class YourAggregate : Entity<YourAggregateId>
{
    public static Result<YourAggregate> Create(/* parameters */)
    {
        // Validation
        var aggregate = new YourAggregate(/* ... */);
        aggregate.RaiseDomainEvent(new YourAggregateCreatedEvent(...));
        return aggregate;
    }
}
```

### How do I add a new command/query?

```csharp
// Command
public record CreateUserCommand(string Email, string Password) : IRequest<Result<Guid>>;

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Implementation
    }
}
```

### How do I add validation?

Use FluentValidation:

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

## Best Practices

### Git workflow

```bash
# Feature branch
git checkout -b feature/your-feature

# Conventional commits
git commit -m "feat: add user registration"
git commit -m "fix: resolve validation issue"
```

### Test structure

Follow the test pyramid: mostly unit tests, some integration tests, few end-to-end tests.

### Naming conventions

- **Classes**: `PascalCase`
- **Methods**: `PascalCase`
- **Properties**: `PascalCase`
- **Private fields**: `_camelCase`
- **Constants**: `PascalCase`

---

## Getting Help

```bash
# Health check
curl http://localhost:4307/healthz

# View all logs
docker compose logs

# Test database
psql -h localhost -U postgres -d lanka_dev -c "SELECT 1;"
```

### Reporting Issues

Include:
1. Steps to reproduce
2. Expected vs actual behavior
3. Environment info (`dotnet --version`, OS, Docker version)
4. Relevant logs

---

<div align="center">

*Still stuck? Open an issue on GitHub.*

</div>
