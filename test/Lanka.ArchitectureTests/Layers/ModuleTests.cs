using System.Reflection;
using Lanka.ArchitectureTests.Abstractions;
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
        string[] otherModules = [CampaignsNamespace];
        string[] integrationEventsModules = [CampaignsIntegrationEventsNamespace];

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
        string[] otherModules = [UsersNamespace];
        string[] integrationEventsModules = [UsersIntegrationEventsNamespace];

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
}
