using System.Reflection;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure;

namespace Lanka.Modules.Analytics.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Analytics.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = typeof(InstagramAccount).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(AnalyticsModule).Assembly;

    protected static readonly Assembly PresentationAssembly = Analytics.Presentation.AssemblyReference.Assembly;
}
