using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Lanka.Gateway.Authentication;

internal sealed class JwtBearerConfigureOptions(IConfiguration configuration) 
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string ConfigurationSectionName = "Authentication";
        
    public void Configure(JwtBearerOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        this.Configure(options);
    }
}
