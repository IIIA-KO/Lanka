using System.Reflection;
using Lanka.Modules.Matching.Domain.SearchableDocuments;
using Lanka.Modules.Matching.Infrastructure;

namespace Lanka.Modules.Matching.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Matching.Application.AssemblyReference.Assembly;
    
    protected static readonly Assembly DomainAssembly = typeof(SearchableDocument).Assembly;
    
    protected static readonly Assembly InfrastructureAssembly = typeof(MatchingModule).Assembly;

    protected static readonly Assembly PresentationAssembly = Matching.Presentation.AssemblyReference.Assembly;
}
