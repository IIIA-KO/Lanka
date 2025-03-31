namespace Lanka.Modules.Users.Infrastructure.Identity;

internal sealed class KeycloakOptions
{
    public string AdminUrl { get; set; }

    public string TokenUrl { get; set; }

    public string ConfidentialClientId { get; set; }

    public string ConfidentialClientSecret { get; set; }
    
    public string PublicClientId { get; set; }
    
    public string BaseUrl { get; set; }
    
    public string Realm { get; set; }
}
