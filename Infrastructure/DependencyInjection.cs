using System.Reflection;

using Application;
using Application.Common.Interfaces.Repositorys;
using Application.Common.Interfaces.Services;
using Application.Interfaces;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using Infrastructure.InternalServices;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Pipelines;
using Infrastructure.Pipelines.Guid;
using Infrastructure.Pipelines.Transaction;
using Infrastructure.Repositorys;

using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        AddPipeline(services);
        AddDatabase(services);
        AddRepositories(services);
        AddInternalServices(services);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private static void AddPipeline(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDatabase(IServiceCollection services)
    {
        services.AddScoped<MigrationManager>();
        services.AddDbContext<SlaisDbContext>();
    }

    private static void AddInternalServices(IServiceCollection services)
    {
        AddMediator(services);
        services.AddSingleton(typeof(ISlaisLogger<>), typeof(SlaisLogger<>));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    private static void AddMediator(IServiceCollection services)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        var handlers = Assembly.GetAssembly(typeof(IApplicationAssemblyMarker))
            ?.GetTypes()
            .Where(t =>
            {
                return t is { IsAbstract: false, IsInterface: false };
            })
            .SelectMany(t =>
            {
                return t.GetInterfaces()
                                    .Where(i =>
                                    {
                                        return i.IsGenericType &&
                                               i.GetGenericTypeDefinition() == handlerInterface;
                                    });
            },
                (type, iface) =>
                {
                    return new { type, iface };
                });

        if (handlers == null)
        {
            return;
        }

        foreach (var handler in handlers)
        {
            services.AddTransient(handler.iface, handler.type);
        }

        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<GuidResolver>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolverPipeline<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionPipeline<,>));
    }

}
