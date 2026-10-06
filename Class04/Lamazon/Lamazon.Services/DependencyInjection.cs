using Lamazon.Services.Abstractions;
using Lamazon.Services.Implementations;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Lamazon.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IProductsService, ProductsService>();
        services.AddScoped<IProductCategoriesService, ProductCategoriesService>();
        services.AddScoped<IUsersService, UsersService>();

        // Mappers
        services.AddMappers();

        return services;
    }

    private static void AddMappers(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.RequireExplicitMapping = true;

        config.Scan(typeof(DependencyInjection).Assembly);

        config.Compile();

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
    }
}
