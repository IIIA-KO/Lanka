using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Lanka.ServiceDefaults;

public static class HealthCheckExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        internal TBuilder AddDefaultHealthChecks()
        {
            IHealthChecksBuilder healthChecks = builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            builder.AddDatabaseHealthCheck(healthChecks);
            builder.AddQueueHealthCheck(healthChecks);
            builder.AddSearchHealthCheck(healthChecks);
            builder.AddKeycloakHealthCheck(healthChecks);

            return builder;
        }

        private void AddDatabaseHealthCheck(IHealthChecksBuilder healthChecks)
        {
            string? connectionString = builder.Configuration.GetConnectionString("Database");

            if (!string.IsNullOrEmpty(connectionString))
            {
                healthChecks.AddNpgSql(connectionString);
            }
        }

        private void AddQueueHealthCheck(IHealthChecksBuilder healthChecks)
        {
            string? connectionString = builder.Configuration.GetConnectionString("Queue");

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            var queueUri = new Uri(connectionString);
            string[] userInfo = queueUri.UserInfo.Split(':', 2);
            string username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0])
                ? Uri.UnescapeDataString(userInfo[0])
                : "guest";
            string password = userInfo.Length > 1
                ? Uri.UnescapeDataString(userInfo[1])
                : "guest";

            builder.Services.TryAddSingleton<IConnection>(_ =>
            {
                var factory = new ConnectionFactory()
                {
                    Uri = queueUri,
                    UserName = username,
                    Password = password,
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            healthChecks.AddRabbitMQ();
        }

        private void AddSearchHealthCheck(IHealthChecksBuilder healthChecks)
        {
            string? connectionString = builder.Configuration.GetConnectionString("lanka-search");

            if (!string.IsNullOrEmpty(connectionString))
            {
                string baseUrl = new Uri(connectionString).GetLeftPart(UriPartial.Authority);
                healthChecks.AddUrlGroup(new Uri(baseUrl), HttpMethod.Get, "elasticsearch");
            }
        }

        private void AddKeycloakHealthCheck(IHealthChecksBuilder healthChecks)
        {
            string? realm = builder.Configuration["KeyCloak:Realm"];

            if (!string.IsNullOrEmpty(realm))
            {
                healthChecks.AddUrlGroup(
                    new Uri($"http://lanka-identity/realms/{realm}"),
                    HttpMethod.Get,
                    "keycloak");
            }
        }
    }
}
