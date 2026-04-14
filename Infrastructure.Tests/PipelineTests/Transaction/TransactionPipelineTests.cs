using Application.Common;
using Application.Utils.Interfaces.Transaction;
using Application.Utils.Logger;

using Domain.Common.Exceptions;

using FluentAssertions;

using Infrastructure.Pipelines.Transaction;
using Infrastructure.Tests.Common;
using Infrastructure.Tests.Common.Repositorys;

using NSubstitute;

using SLAIS.Domain.Users;

using Xunit;

namespace Infrastructure.Tests.PipelineTests.Transaction;

public class TransactionPipelineTests : TestBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlaisLogger<TransactionPipeline<TestTransactionRequest, bool>> _logger;
    private readonly ISlaisLogger<TransactionPipeline<TestNoTransactionRequest, bool>> _noTransactionLogger;
    private readonly UserTestRepository _userTestRepository;
    private readonly InstituteTestRepository _instituteTestRepository;

    public TransactionPipelineTests(PostgreSqlContainerFixture fixture)
        : base(fixture)
    {
        _unitOfWork = new UnitOfWork(fixture.SlaisDbContext);
        _logger = Substitute.For<ISlaisLogger<TransactionPipeline<TestTransactionRequest, bool>>>();
        _noTransactionLogger = Substitute.For<ISlaisLogger<TransactionPipeline<TestNoTransactionRequest, bool>>>();
        _userTestRepository = new UserTestRepository(fixture);
        _instituteTestRepository = new InstituteTestRepository(fixture);
    }

    #region WithTransaction - Erfolg

    [Fact]
    public async Task StartWithTransactionAsync_ShouldCommit_WhenNextSucceeds()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var pipeline = new TransactionPipeline<TestTransactionRequest, bool>(
            _unitOfWork,
            _logger);

        var request = new TestTransactionRequest();

        UserEntity? createdUser = null;

        var result = await pipeline.HandleAsync(
            request,
            async () =>
            {
                createdUser = await _userTestRepository
                    .CreateAdminAsync(institute.Guid);

                return true;
            },
            CancellationToken.None);

        result.Should().BeTrue();

        var persistedUser = await GetCreatedEntityByGuid<UserEntity>(createdUser!.Guid);
        persistedUser.Should().NotBeNull();
    }

    #endregion

    #region WithTransaction - Rollback bei SlaisException

    [Fact]
    public async Task StartWithTransactionAsync_ShouldRollback_WhenSlaisExceptionIsThrown()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var pipeline = new TransactionPipeline<TestTransactionRequest, bool>(
            _unitOfWork,
            _logger);

        var request = new TestTransactionRequest();

        UserEntity? createdUser = null;

        var act = async () => await pipeline.HandleAsync(
            request,
            async () =>
            {
                createdUser = await _userTestRepository
                    .CreateAdminAsync(institute.Guid);

                throw new SlaisException(CommonErrorCodes.DefaultErrorCode);
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);

        var persistedUser = await GetCreatedEntityByGuid<UserEntity>(createdUser!.Guid);
        persistedUser.Should().BeNull();
    }

    #endregion

    #region WithTransaction - Rollback bei unbekannter Exception

    [Fact]
    public async Task StartWithTransactionAsync_ShouldRollbackAndWrapException_WhenUnknownExceptionIsThrown()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var pipeline = new TransactionPipeline<TestTransactionRequest, bool>(
            _unitOfWork,
            _logger);

        var request = new TestTransactionRequest();

        UserEntity? createdUser = null;

        var act = async () => await pipeline.HandleAsync(
            request,
            async () =>
            {
                createdUser = await _userTestRepository
                    .CreateAdminAsync(institute.Guid);

                throw new InvalidOperationException("Unbekannter Fehler");
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);

        var persistedUser = await GetCreatedEntityByGuid<UserEntity>(createdUser!.Guid);
        persistedUser.Should().BeNull();
    }

    #endregion

    #region WithoutTransaction - Erfolg

    [Fact]
    public async Task StartWithoutTransactionAsync_ShouldSaveChanges_WhenNextSucceeds()
    {
        var institute = await _instituteTestRepository
            .CreateInstituteAsync();

        var pipeline = new TransactionPipeline<TestNoTransactionRequest, bool>(
            _unitOfWork,
            _noTransactionLogger);

        var request = new TestNoTransactionRequest();

        UserEntity? createdUser = null;

        var result = await pipeline.HandleAsync(
            request,
            async () =>
            {
                createdUser = await _userTestRepository
                    .CreateAdminAsync(institute.Guid);

                return true;
            },
            CancellationToken.None);

        result.Should().BeTrue();

        var persistedUser = await GetCreatedEntityByGuid<UserEntity>(createdUser!.Guid);
        persistedUser.Should().NotBeNull();
    }

    #endregion

    #region WithoutTransaction - SlaisException wird durchgereicht

    [Fact]
    public async Task StartWithoutTransactionAsync_ShouldRethrowSlaisException_WhenSlaisExceptionIsThrown()
    {
        var pipeline = new TransactionPipeline<TestNoTransactionRequest, bool>(
            _unitOfWork,
            _noTransactionLogger);

        var request = new TestNoTransactionRequest();

        var act = async () => await pipeline.HandleAsync(
            request,
            () => throw new SlaisException(CommonErrorCodes.DefaultErrorCode),
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);
    }

    #endregion

    #region WithoutTransaction - Unbekannte Exception wird gewrappt

    [Fact]
    public async Task StartWithoutTransactionAsync_ShouldWrapException_WhenUnknownExceptionIsThrown()
    {
        var pipeline = new TransactionPipeline<TestNoTransactionRequest, bool>(
            _unitOfWork,
            _noTransactionLogger);

        var request = new TestNoTransactionRequest();

        var act = async () => await pipeline.HandleAsync(
            request,
            () => throw new InvalidOperationException("Unbekannter Fehler"),
            CancellationToken.None);

        await act.Should().ThrowAsync<SlaisException>()
            .Where(e => e.ErrorCode == (int)CommonErrorCodes.DefaultErrorCode);
    }

    #endregion
}
