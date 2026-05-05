using Lanka.AppHost;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// --- Parameters (stored in user secrets) ---

IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("pg-password", secret: true);
IResourceBuilder<ParameterResource> rabbitPassword = builder.AddParameter("rabbitmq-password", secret: true);
IResourceBuilder<ParameterResource> keycloakPassword = builder.AddParameter("keycloak-password", secret: true);
IResourceBuilder<ParameterResource> mongoPassword = builder.AddParameter("mongo-password", secret: true);
IResourceBuilder<ParameterResource> ngrokAuthToken = builder.AddParameter("ngrok-auth-token", secret: true);
IResourceBuilder<ParameterResource> wayForPayPublicBaseUrl = builder.AddParameter("wayforpay-public-base-url", secret: true);

string? configuredNgrokAuthToken = builder.Configuration["Parameters:ngrok-auth-token"];
string? configuredWayForPayPublicBaseUrl = builder.Configuration["Parameters:wayforpay-public-base-url"];
bool shouldStartWayForPayTunnel =
    !string.IsNullOrWhiteSpace(configuredNgrokAuthToken)
    && configuredNgrokAuthToken != "replace-with-ngrok-auth-token"
    && !string.IsNullOrWhiteSpace(configuredWayForPayPublicBaseUrl)
    && configuredWayForPayPublicBaseUrl != "https://replace-with-static-ngrok-domain.ngrok-free.app";

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

if (shouldStartWayForPayTunnel)
{
    api.WithEnvironment("Campaigns__WayForPay__PublicBaseUrl", wayForPayPublicBaseUrl);
}

IResourceBuilder<ProjectResource> gateway = builder
    .AddProject<Projects.Lanka_Gateway>("lanka-gateway")
    .WithEndpoint("https", endpoint => endpoint.Port = 4308)
    .WithReference(api)
    .WithReference(keycloak)
    .WaitFor(api)
    .WaitFor(keycloak);

if (shouldStartWayForPayTunnel)
{
    builder
        .AddContainer(
            "wayforpay-tunnel",
            "ngrok/ngrok",
            "latest")
        .WithEnvironment("NGROK_AUTHTOKEN", ngrokAuthToken)
        .WithArgs(
            "http",
            "https://host.docker.internal:4308",
            "--url",
            wayForPayPublicBaseUrl,
            "--upstream-tls-verify=false")
        .WaitFor(gateway);
}

// --- Frontend ---

builder.AddJavaScriptApp("lanka-client", "../../../client/lanka-client", "start")
    .WithReference(gateway)
    .WithHttpsEndpoint(port: 4200, targetPort: 4200, isProxied: false)
    .WaitFor(gateway);

await builder.Build().RunAsync();
