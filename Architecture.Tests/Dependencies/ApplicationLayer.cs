using NetArchTest.Rules;

namespace Architecture.Tests.Dependencies;

public class ApplicationLayer : TestBase
{
    [Fact]
    public void Application_ShouldNot_HaveAnyForbiddenExternalDependencies()
    {

        var result = Types
            .InAssembly(typeof(Application.IApplicationAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(_forbiddenAssembliesForApplication)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Types in Application have forbidden dependencies:{Environment.NewLine}{LogForbiddenDependencies(result)}");
    }

    [Fact]
    public void Application_ShouldHave_HaveAllowedExternalDependencies()
    {
        var referencedAssemblies = typeof(Application.IApplicationAssemblyMarker)
            .Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.True(_allowedAssembliesForApplication
            .All(allowed => referencedAssemblies.Contains(allowed)));
    }
}
