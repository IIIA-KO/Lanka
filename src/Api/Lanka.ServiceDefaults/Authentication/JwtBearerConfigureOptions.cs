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

    public void Configure(JwtBearerOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);

        string? realm = configuration["KeyCloak:Realm"];

        if (string.IsNullOrEmpty(realm))
        {
            return;
        }

        string realmUrl = $"http://lanka-identity/realms/{realm}";

        options.MetadataAddress = $"{realmUrl}/.well-known/openid-configuration";
        options.Backchannel = httpClientFactory.CreateClient("keycloak-backchannel");
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        this.Configure(options);
    }
}
