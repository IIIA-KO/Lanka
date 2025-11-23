using System.Reflection;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Infrastructure;

namespace Lanka.Modules.Matching.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly =
        typeof(Matching.Application.AssemblyReference).Assembly;

    protected static readonly Assembly DomainAssembly
        = typeof(SearchableItemType).Assembly;

    protected static readonly Assembly InfrastructureAssembly
        = typeof(MatchingModule).Assembly;

    protected static readonly Assembly PresentationAssembly
        = typeof(Matching.Presentation.AssemblyReference).Assembly;
}
