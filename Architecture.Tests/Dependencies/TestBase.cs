using NetArchTest.Rules;

namespace Architecture.Tests.Dependencies;

public abstract class TestBase
{
    private static readonly string? _domainAssembly = typeof(Domain.IDomainAssemblyMarker).Assembly.GetName().Name;

    private static readonly string? _applicationAssembly = typeof(Application.IApplicationAssemblyMarker).Assembly.GetName().Name;

    private static readonly string? _infrastructureAssembly = typeof(Infrastructure.IInfrastructureAssemblyMarker).Assembly.GetName().Name;

    private static readonly string? _presentationAssembly = typeof(Presentation.IPresentationAssemblyMarker).Assembly.GetName().Name;

    #region Common

    protected static string LogForbiddenDependencies(TestResult result)
    {
        var failingTypes = result.FailingTypes?
            .Select(t => t.FullName)
            .ToList() ?? [];

        var message = string.Join(Environment.NewLine, failingTypes);

        return message;
    }

    #endregion

    #region Domain

    protected readonly string?[] _forbiddenAssembliesForDomain =
    [
        _applicationAssembly, _infrastructureAssembly, _presentationAssembly
    ];

    protected readonly string?[] _allowedAssembliesForDomain =
    [
    ];

    #endregion

    #region Application

    protected readonly string?[] _forbiddenAssembliesForApplication =
    [
        _infrastructureAssembly, _presentationAssembly
    ];

    protected readonly string?[] _allowedAssembliesForApplication =
    [
        _domainAssembly
    ];

    #endregion

    #region Infrastructure

    protected readonly string?[] _forbiddenAssembliesForInfrastructure =
    [
        _presentationAssembly
    ];

    protected readonly string?[] _allowedAssembliesForInfrastructure =
    [
        _domainAssembly, _applicationAssembly
    ];

    #endregion

    #region Presentation

    protected readonly string?[] _forbiddenAssembliesForPresentation =
    [
    ];

    protected readonly string?[] _allowedAssembliesForPresentation =
    [
        _domainAssembly, _applicationAssembly, _infrastructureAssembly
    ];

    #endregion
}
