using Lanka.AppHost;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// --- Parameters (stored in user secrets) ---

IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("pg-password", secret: true);
IResourceBuilder<ParameterResource> rabbitPassword = builder.AddParameter("rabbitmq-password", secret: true);
IResourceBuilder<ParameterResource> keycloakPassword = builder.AddParameter("keycloak-password", secret: true);
IResourceBuilder<ParameterResource> mongoPassword = builder.AddParameter("mongo-password", secret: true);

// --- Infrastructure ---

(IResourceBuilder<PostgresServerResource> postgres, IResourceBuilder<PostgresDatabaseResource> lankaDb) = builder.AddLankaPostgres(pgPassword);
IResourceBuilder<RedisResource> redis = builder.AddLankaCache();
IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddLankaQueue(rabbitPassword);
(IResourceBuilder<MongoDBServerResource> mongo, IResourceBuilder<MongoDBDatabaseResource> mongoDb) = builder.AddLankaMongo(mongoPassword);
IResourceBuilder<ElasticsearchResource> elasticsearch = builder.AddLankaSearch();
IResourceBuilder<KeycloakResource> keycloak = builder.AddLankaIdentity(keycloakPassword);

builder.AddLankaKibana();

// --- Application projects ---

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.Lanka_Api>("lanka-api")
    .WithScalar()
    .WithReference(lankaDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(mongoDb)
    .WithReference(elasticsearch)
    .WithReference(keycloak)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(mongo)
    .WaitFor(elasticsearch)
    .WaitFor(keycloak);

IResourceBuilder<ProjectResource> gateway = builder
    .AddProject<Projects.Lanka_Gateway>("lanka-gateway")
    .WithEndpoint("https", endpoint => endpoint.Port = 4308)
    .WithReference(api)
    .WithReference(keycloak)
    .WaitFor(api)
    .WaitFor(keycloak);

// --- Frontend ---

builder.AddJavaScriptApp("lanka-client", "../../../client/lanka-client", "start")
    .WithReference(gateway)
    .WithHttpsEndpoint(port: 4200, targetPort: 4200, isProxied: false)
    .WaitFor(gateway);

await builder.Build().RunAsync();
