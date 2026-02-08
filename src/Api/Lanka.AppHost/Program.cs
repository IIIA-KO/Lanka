IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// --- Parameters (stored in user secrets) ---

IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("pg-password", secret: true);
IResourceBuilder<ParameterResource> rabbitPassword = builder.AddParameter("rabbitmq-password", secret: true);
IResourceBuilder<ParameterResource> keycloakPassword = builder.AddParameter("keycloak-password", secret: true);
IResourceBuilder<ParameterResource> mongoPassword = builder.AddParameter("mongo-password", secret: true);

// --- Infrastructure ---

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("lanka-postgres", port: 5432, password: pgPassword)
    .WithImageTag("17.6")
    .WithContainerName("lanka-postgres")
    .WithDataVolume();
IResourceBuilder<PostgresDatabaseResource> lankaDb = postgres.AddDatabase("Database", databaseName: "lanka");

#pragma warning disable ASPIRECERTIFICATES001
IResourceBuilder<RedisResource> redis = builder.AddRedis("Cache", port: 6379)
    .WithImageTag("8.2")
    .WithContainerName("lanka-cache")
    .WithDataVolume()
    .WithoutHttpsCertificate();
#pragma warning restore ASPIRECERTIFICATES001

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("Queue", port: 5672, password: rabbitPassword)
    .WithImageTag("4.2")
    .WithContainerName("lanka-queue")
    .WithManagementPlugin()
    .WithDataVolume();

IResourceBuilder<MongoDBServerResource> mongo = builder.AddMongoDB("lanka-mongo", port: 27017, password: mongoPassword)
    .WithImageTag("8.2")
    .WithContainerName("lanka-mongo")
    .WithDataVolume();
IResourceBuilder<MongoDBDatabaseResource> mongoDb = mongo.AddDatabase("Mongo");

IResourceBuilder<ElasticsearchResource> elasticsearch = builder.AddElasticsearch("lanka-search")
    .WithImageTag("9.1.10")
    .WithContainerName("lanka-search")
    .WithDataVolume()
    .WithVolume("lanka-search-plugins", "/usr/share/elasticsearch/plugins")
    .WithBindMount("elasticsearch-plugins.yml",
        "/usr/share/elasticsearch/config/elasticsearch-plugins.yml")
    .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithEnvironment("xpack.security.enrollment.enabled", "false");

// Replace the built-in health check (uses ElasticsearchClient with security credentials,
foreach (HealthCheckAnnotation annotation in elasticsearch.Resource.Annotations.OfType<HealthCheckAnnotation>().ToList())
{
    elasticsearch.Resource.Annotations.Remove(annotation);
}

elasticsearch.WithHttpHealthCheck("/");

IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak("lanka-identity", port: 18080, adminPassword: keycloakPassword)
    .WithImageTag("26.4")
    .WithContainerName("lanka-identity")
    .WithRealmImport("../../../.files")
    .WithDataVolume()
    .WithEnvironment("KC_HEALTH_ENABLED", "true");

// Replace the built-in health check (checks management HTTPS port, fails due to self-signed cert)
foreach (HealthCheckAnnotation annotation in keycloak.Resource.Annotations.OfType<HealthCheckAnnotation>().ToList())
{
    keycloak.Resource.Annotations.Remove(annotation);
}

keycloak.WithHttpHealthCheck("/realms/lanka");

builder.AddContainer("lanka-kibana", "kibana", "9.1.10")
    .WithContainerName("lanka-kibana")
    .WithEnvironment("ELASTICSEARCH_HOSTS", "[\"http://lanka-search:9200\"]")
    .WithHttpEndpoint(port: 5601, targetPort: 5601, isProxied: false);

// --- Application projects ---

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Lanka_Api>("lanka-api")
    .WithReference(lankaDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(mongoDb)
    .WithReference(elasticsearch)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(mongo)
    .WaitFor(elasticsearch)
    .WaitFor(keycloak);

IResourceBuilder<ProjectResource> gateway = builder.AddProject<Projects.Lanka_Gateway>("lanka-gateway")
    .WithEndpoint("https", endpoint => endpoint.Port = 4308)
    .WithReference(api)
    .WaitFor(api)
    .WaitFor(keycloak);

// --- Frontend ---

builder.AddJavaScriptApp("lanka-client", "../../../client/lanka-client", "start")
    .WithReference(gateway)
    .WithHttpsEndpoint(port: 4200, targetPort: 4200, isProxied: false)
    .WaitFor(gateway);

await builder.Build().RunAsync();
