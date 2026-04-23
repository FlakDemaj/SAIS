using NetArchTest.Rules;

namespace Architecture.Tests.Dependencies;

public class DomainLayer : TestBase
{
    [Fact]
    public void Domain_ShouldNot_HaveAnyForbiddenExternalDependencies()
    {
        var result = Types
            .InAssembly(typeof(Domain.IDomainAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(_forbiddenAssembliesForDomain)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Types in Domain have forbidden dependencies:{Environment.NewLine}{LogForbiddenDependencies(result)}");
    }

    [Fact]
    public void Presentation_ShouldHave_HaveAllowedExternalDependencies()
    {
        var referencedAssemblies = typeof(Domain.IDomainAssemblyMarker)
            .Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.True(_allowedAssembliesForDomain
            .All(allowed => referencedAssemblies.Contains(allowed)));
    }
}
