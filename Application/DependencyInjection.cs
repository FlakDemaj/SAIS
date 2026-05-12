using Domain.Public.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{

    public static void AddApplicationLayer(this IServiceCollection services)
    {
        AutoAutoMapper(services);

        services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
    }

    private static void AutoAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(IApplicationAssemblyMarker).Assembly);
    }
}
