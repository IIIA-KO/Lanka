namespace Lanka.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected const string UsersNamespace = "Lanka.Modules.Users";
    protected const string UsersIntegrationEventsNamespace = "Lanka.Modules.Users.IntegrationEvents";

    protected const string CampaignsNamespace = "Lanka.Modules.Campaigns";
    protected const string CampaignsIntegrationEventsNamespace = "Lanka.Modules.Campaigns.IntegrationEvents";
}
