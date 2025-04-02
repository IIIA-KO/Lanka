using System.Reflection;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Infrastructure;

namespace Lanka.Modules.Users.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // "Types can be made internal"
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Users.Application.AssemblyReference.Assembly;
    
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    
    protected static readonly Assembly InfrastructureAssembly = typeof(UsersModule).Assembly;

    protected static readonly Assembly PresentationAssembly = Users.Presentation.AssemblyReference.Assembly;
}
