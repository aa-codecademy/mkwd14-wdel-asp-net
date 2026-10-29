using FluentValidation;
using Lamazon.Domain.Entities;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Implementations;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Lamazon.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IProductsService, ProductsService>();
        services.AddScoped<IProductCategoriesService, ProductCategoriesService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IOrdersService, OrdersService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddGeoTrackerService();

        // Mappers
        services.AddMappers();

        // Validators
        services.AddValidators();

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

    private static void AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
    }

    private static void AddGeoTrackerService(this IServiceCollection services)
    {
        services.AddHttpClient<IGeoTrackerService, GeoTrackerService>(client =>
        {
            client.BaseAddress = new Uri("http://ip-api.com/");
        });
    }

}
