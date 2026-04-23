using NetArchTest.Rules;

namespace Architecture.Tests.Dependencies;

public class PresentationLayer : TestBase
{
    [Fact]
    public void Presentation_ShouldNot_HaveAnyForbiddenExternalDependencies()
    {
        var result = Types
            .InAssembly(typeof(Presentation.IPresentationAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(_forbiddenAssembliesForPresentation)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Types in Presentation have forbidden dependencies:{Environment.NewLine}{LogForbiddenDependencies(result)}");
    }

    [Fact]
    public void Presentation_ShouldHave_HaveAllowedExternalDependencies()
    {
        var referencedAssemblies = typeof(Presentation.IPresentationAssemblyMarker)
            .Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.True(_allowedAssembliesForPresentation
            .All(allowed => referencedAssemblies.Contains(allowed)));
    }
}
