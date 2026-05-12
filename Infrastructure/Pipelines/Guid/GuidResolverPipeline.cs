using Application.Common.Interfaces;

namespace Infrastructure.Pipelines.Guid;

public class GuidResolverPipeline<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly GuidResolver _resolver;

    public GuidResolverPipeline(GuidResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {

        if (request is not IHasGuid hasGuid
            || hasGuid.Guid != System.Guid.Empty)
        {
            return await next();
        }

        var guid = await _resolver.ResolveAsync(
            hasGuid.PublicId,
            hasGuid.EntityType,
            cancellationToken);

        hasGuid.Guid = guid;

        return await next();
    }
}
