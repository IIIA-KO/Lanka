using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Lanka.ServiceDefaults.Authentication;

public class JwtBearerConfigureOptions(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory
) : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string ConfigurationSectionName = "Authentication";
    internal const string KeycloakServiceDiscoveryKey = "services:lanka-identity:http:0";

    public void Configure(JwtBearerOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);

        string? realm = configuration["KeyCloak:Realm"];

        if (string.IsNullOrEmpty(realm))
        {
            return;
        }

        string keycloakBaseUrl = GetKeycloakBaseUrl(configuration);
        string realmUrl = $"{keycloakBaseUrl}/realms/{realm}";

        options.Authority = realmUrl;
        options.MetadataAddress = $"{realmUrl}/.well-known/openid-configuration";
        options.Backchannel = httpClientFactory.CreateClient("keycloak-backchannel");
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        this.Configure(options);
    }

    internal static string GetKeycloakBaseUrl(IConfiguration configuration)
    {
        return configuration[KeycloakServiceDiscoveryKey] ?? $"http://{KeycloakServiceName}";
    }

    private const string KeycloakServiceName = "lanka-identity";
}
