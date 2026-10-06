using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.DataAccess.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lamazon.DataAccess;

/// <summary>
/// Registers everything the data layer owns: the Context and the repositories.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LamazonDb");

        // Database
        services.AddDbContext<LamazonDbContext>(options => options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IProductCategoriesRepository, ProductCategoriesRepository>();


        return services;
    }
}
