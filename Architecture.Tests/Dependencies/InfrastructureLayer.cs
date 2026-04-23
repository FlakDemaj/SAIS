using NetArchTest.Rules;

namespace Architecture.Tests.Dependencies;

public class InfrastructureLayer : TestBase
{
    [Fact]
    public void Infrastructure_ShouldNot_HaveAnyForbiddenExternalDependencies()
    {
        var result = Types
            .InAssembly(typeof(Infrastructure.IInfrastructureAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(_forbiddenAssembliesForInfrastructure)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Types in Infrastructure have forbidden dependencies:{Environment.NewLine}{LogForbiddenDependencies(result)}");
    }

    [Fact]
    public void Infrastructure_ShouldHave_HaveAllowedExternalDependencies()
    {
        var referencedAssemblies = typeof(Infrastructure.IInfrastructureAssemblyMarker)
            .Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.True(_allowedAssembliesForInfrastructure
            .All(allowed => referencedAssemblies.Contains(allowed)));
    }
}
