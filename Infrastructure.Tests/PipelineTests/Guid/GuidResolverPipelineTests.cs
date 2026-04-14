using Application.Common;

using Domain.Common.Exceptions;

using FluentAssertions;

using Infrastructure.Pipelines.Guid;
using Infrastructure.Tests.Common;
using Infrastructure.Tests.Common.Repositorys;

using Xunit;

namespace Infrastructure.Tests.PipelineTests.Guid;

public class GuidResolverTests : TestBase
{
    private readonly GuidResolver _guidResolver;
    private readonly UserTestRepository _userTestRepository;
    private readonly InstituteTestRepository _instituteTestRepository;

    public GuidResolverTests(PostgreSqlContainerFixture fixture)
        : base(fixture)
    {
        _guidResolver = new GuidResolver(fixture.SlaisDbContext);
        _userTestRepository = new UserTestRepository(fixture);
        _instituteTestRepository = new InstituteTestRepository(fixture);
    }

    #region ResolveAsync - User gefunden

    [Fact]
    public async Task ResolveAsync_ShouldReturnGuid_WhenUserExistsWithPublicId()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var user = await _userTestRepository
            .CreateAdminAsync(institute.Guid);

        var result = await _guidResolver.ResolveAsync(
            (int)user.Id!,
            "User",
            CancellationToken.None);

        result.Should().Be(user.Guid);
    }

    #endregion

    #region ResolveAsync - User nicht gefunden

    [Fact]
    public async Task ResolveAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var act = async () => await _guidResolver.ResolveAsync(
            999,
            "User",
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);
    }

    #endregion

    #region ResolveAsync - Unbekannter EntityType

    [Fact]
    public async Task ResolveAsync_ShouldThrow_WhenEntityTypeIsUnknown()
    {
        var act = async () => await _guidResolver.ResolveAsync(
            1,
            "UnknownEntity",
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);
    }

    [Fact]
    public async Task ResolveAsync_ShouldThrow_WhenEntityTypeIsEmpty()
    {
        var act = async () => await _guidResolver.ResolveAsync(
            1,
            string.Empty,
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);
    }

    #endregion

    #region GuidResolverPipeline - Next wird aufgerufen

    [Fact]
    public async Task GuidResolverPipeline_ShouldSetGuidOnRequest_WhenPublicIdIsValid()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var user = await _userTestRepository
            .CreateAdminAsync(institute.Guid);

        var pipeline = new GuidResolverPipeline<TestRequest, bool>(_guidResolver);

        var request = new TestRequest
        {
            PublicId = (int)user.Id!,
            EntityType = "User"
        };

        var nextWasCalled = false;

        await pipeline.HandleAsync(
            request,
            () =>
            {
                nextWasCalled = true;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        request.Guid.Should().Be(user.Guid);
        nextWasCalled.Should().BeTrue();
    }

    #endregion

    #region GuidResolverPipeline - Next wird nicht aufgerufen

    [Fact]
    public async Task GuidResolverPipeline_ShouldThrow_WhenPublicIdDoesNotExist()
    {
        var pipeline = new GuidResolverPipeline<TestRequest, bool>(_guidResolver);

        var request = new TestRequest
        {
            PublicId = 999,
            EntityType = "User"
        };

        var act = async () => await pipeline.HandleAsync(
            request,
            () => Task.FromResult(true),
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);
    }

    [Fact]
    public async Task GuidResolverPipeline_ShouldNotCallNext_WhenResolveFails()
    {
        var pipeline = new GuidResolverPipeline<TestRequest, bool>(_guidResolver);

        var request = new TestRequest
        {
            PublicId = 999,
            EntityType = "User"
        };

        var nextWasCalled = false;

        var act = async () => await pipeline.HandleAsync(
            request,
            () =>
            {
                nextWasCalled = true;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>();
        nextWasCalled.Should().BeFalse();
    }

    #endregion
}
