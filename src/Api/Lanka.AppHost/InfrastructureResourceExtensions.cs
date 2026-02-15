namespace Lanka.AppHost;

internal static class InfrastructureResourceExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal (IResourceBuilder<PostgresServerResource> Server, IResourceBuilder<PostgresDatabaseResource> Database)
            AddLankaPostgres(IResourceBuilder<ParameterResource> password)
        {
            IResourceBuilder<PostgresServerResource> postgres = builder
                .AddPostgres("lanka-postgres", port: 5432, password: password)
                .WithImageTag("17.6")
                .WithContainerName("lanka-postgres")
                .WithDataVolume();

            IResourceBuilder<PostgresDatabaseResource> database = postgres
                .AddDatabase("Database", databaseName: "lanka");

            return (postgres, database);
        }

#pragma warning disable ASPIRECERTIFICATES001
        internal IResourceBuilder<RedisResource> AddLankaCache()
        {
            return builder
                .AddRedis("Cache", port: 6379)
                .WithImageTag("8.2")
                .WithContainerName("lanka-cache")
                .WithDataVolume()
                .WithoutHttpsCertificate();
        }
#pragma warning restore ASPIRECERTIFICATES001

        internal IResourceBuilder<RabbitMQServerResource> AddLankaQueue(IResourceBuilder<ParameterResource> password)
        {
            return builder
                .AddRabbitMQ("Queue", port: 5672, password: password)
                .WithImageTag("4.2")
                .WithContainerName("lanka-queue")
                .WithManagementPlugin()
                .WithDataVolume();
        }

        internal (IResourceBuilder<MongoDBServerResource> Server, IResourceBuilder<MongoDBDatabaseResource> Database)
            AddLankaMongo(IResourceBuilder<ParameterResource> password)
        {
            IResourceBuilder<MongoDBServerResource> mongo = builder
                .AddMongoDB("lanka-mongo", port: 27017, password: password)
                .WithImageTag("8.2")
                .WithContainerName("lanka-mongo")
                .WithDataVolume();

            IResourceBuilder<MongoDBDatabaseResource> database = mongo.AddDatabase("Mongo");

            return (mongo, database);
        }

        internal IResourceBuilder<ElasticsearchResource> AddLankaSearch()
        {
            return builder
                .AddElasticsearch("lanka-search")
                .WithImageTag("9.1.10")
                .WithContainerName("lanka-search")
                .WithDataVolume()
                .WithVolume("lanka-search-plugins", "/usr/share/elasticsearch/plugins")
                .WithBindMount(
                    "elasticsearch-plugins.yml",
                    "/usr/share/elasticsearch/config/elasticsearch-plugins.yml")
                .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
                .WithEnvironment("xpack.security.enabled", "false")
                .WithEnvironment("xpack.security.enrollment.enabled", "false")
                .WithReplacedHealthCheck("/");
        }

        internal IResourceBuilder<KeycloakResource> AddLankaIdentity(
            IResourceBuilder<ParameterResource> password)
        {
            return builder
                .AddKeycloak("lanka-identity", port: 18080, adminPassword: password)
                .WithImageTag("26.4")
                .WithContainerName("lanka-identity")
                .WithRealmImport("../../../.files")
                .WithDataVolume()
                .WithEnvironment("KC_HEALTH_ENABLED", "true")
                .WithReplacedHealthCheck("/realms/lanka");
        }

        internal void AddLankaKibana()
        {
            builder.AddContainer("lanka-kibana", "kibana", "9.1.10")
                .WithContainerName("lanka-kibana")
                .WithEnvironment("ELASTICSEARCH_HOSTS", "[\"http://lanka-search:9200\"]")
                .WithHttpEndpoint(port: 5601, targetPort: 5601, isProxied: false);
        }
    }
}
