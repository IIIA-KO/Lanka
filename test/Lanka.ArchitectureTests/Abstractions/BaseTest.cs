namespace Lanka.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected const string UsersNamespace = "Lanka.Modules.Users";
    protected const string UsersIntegrationEventsNamespace = "Lanka.Modules.Users.IntegrationEvents";

    protected const string CampaignsNamespace = "Lanka.Modules.Campaigns";
    protected const string CampaignsIntegrationEventsNamespace = "Lanka.Modules.Campaigns.IntegrationEvents";
    
    protected const string AnalyticsNamespace = "Lanka.Modules.Analytics";
    protected const string AnalyticsIntegrationEventsNamespace = "Lanka.Modules.Analytics.IntegrationEvents";
    
    protected const string MatchingNamespace = "Lanka.Modules.Matching";
    protected const string MatchingIntegrationEventsNamespace = "Lanka.Modules.Matching.IntegrationEvents";
}
