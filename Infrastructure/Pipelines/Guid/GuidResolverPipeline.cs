using Application.Common.Interfaces;

namespace Infrastructure.Pipelines.Guid;

public class GuidResolverPipeline<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid
{
    private readonly Guid.GuidResolver _resolver;

    public GuidResolverPipeline(Guid.GuidResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        var guid = await _resolver.ResolveAsync(
            request.PublicId,
            request.EntityType,
            cancellationToken);

        request.Guid = guid;

        return await next();
    }

}
