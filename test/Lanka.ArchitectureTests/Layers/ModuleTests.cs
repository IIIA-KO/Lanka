using System.Reflection;
using Lanka.ArchitectureTests.Abstractions;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Infrastructure;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Infrastructure;
using NetArchTest.Rules;

namespace Lanka.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    [Fact]
    public void UsersModule_ShouldNotHaveDependenciesOnOtherModules()
    {
        string[] otherModules = [CampaignsNamespace, AnalyticsNamespace];
        string[] integrationEventsModules = [CampaignsIntegrationEventsNamespace, AnalyticsIntegrationEventsNamespace];

        List<Assembly> userAssemblies =
        [
            typeof(User).Assembly,
            Modules.Users.Application.AssemblyReference.Assembly,
            Modules.Users.Presentation.AssemblyReference.Assembly,
            typeof(UsersModule).Assembly,
        ];

        Types.InAssemblies(userAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
    
    [Fact]
    public void CampaignsModule_ShouldNotHaveDependenciesOnOtherModules()
    {
        string[] otherModules = [UsersNamespace, AnalyticsNamespace];
        string[] integrationEventsModules = [UsersIntegrationEventsNamespace, AnalyticsNamespace];

        List<Assembly> campaignAssemblies =
        [
            typeof(Campaign).Assembly,
            Modules.Campaigns.Application.AssemblyReference.Assembly,
            Modules.Campaigns.Presentation.AssemblyReference.Assembly,
            typeof(CampaignsModule).Assembly,
        ];

        Types.InAssemblies(campaignAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void AnalyticsModule_ShouldNotHaveDependenciesOnOtherModules()
    {
        string[] otherModules = [UsersNamespace, CampaignsNamespace];
        string[] integrationEventsModules = [UsersIntegrationEventsNamespace, CampaignsNamespace];
        
        List<Assembly> analyticsAssemblies =
        [
            typeof(InstagramAccount).Assembly,
            Modules.Analytics.Application.AssemblyReference.Assembly,
            Modules.Analytics.Presentation.AssemblyReference.Assembly,
            typeof(AnalyticsModule).Assembly,
        ];

        Types.InAssemblies(analyticsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
