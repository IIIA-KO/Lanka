using System.Reflection;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Infrastructure;

namespace Lanka.Modules.Campaigns.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Campaigns.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = typeof(Campaign).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(CampaignsModule).Assembly;

    protected static readonly Assembly PresentationAssembly = Campaigns.Presentation.AssemblyReference.Assembly;
}
