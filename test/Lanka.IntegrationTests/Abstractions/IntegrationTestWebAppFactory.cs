using System.Net.Http.Json;
using Lanka.IntegrationTests.Users;
using Lanka.Modules.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Lanka.IntegrationTests.Abstractions;

#pragma warning disable CA1515 // Type can be made internal
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("lanka")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:latest")
        .Build();

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder("quay.io/keycloak/keycloak:latest")
        .WithResourceMapping(
            new FileInfo("lanka-realm-export.json"),
            new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:Database", this._dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings:Cache", this._redisContainer.GetConnectionString());

        string keycloakAddress = this._keycloakContainer.GetBaseAddress();
        string keyCloakRealmUrl = $"{keycloakAddress}realms/lanka";

        Environment.SetEnvironmentVariable(
            "Authentication:MetadataAddress",
            $"{keyCloakRealmUrl}/.well-known/openid-configuration"
        );

        Environment.SetEnvironmentVariable(
            "Authentication:TokenValidationParameters:ValidIssuer",
            keyCloakRealmUrl
        );

        builder.ConfigureTestServices(services =>
            services.Configure<KeycloakOptions>(o =>
            {
                o.AdminUrl = $"{keycloakAddress}admin/realms/lanka/";
                o.TokenUrl = $"{keyCloakRealmUrl}/protocol/openid-connect/token";
            })
        );
    }

    public async Task InitializeAsync()
    {
        await this._dbContainer.StartAsync();
        await this._redisContainer.StartAsync();
        await this._keycloakContainer.StartAsync();

        await this.InitializeTestUserAsync();
    }

    public new async Task DisposeAsync()
    {
        await this._dbContainer.StopAsync();
        await this._redisContainer.StopAsync();
        await this._keycloakContainer.StopAsync();
    }

    private async Task InitializeTestUserAsync()
    {
        try
        {
            using HttpClient httpClient = this.CreateClient();

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "users/register",
                UserData.RegisterTestUserRequest
            );

            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"User registration failed: {response.StatusCode}, {responseBody}"
                );
            }
        }
        catch
        {
            // Do nothing.
        }
    }
}
