using Lamazon.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lamazon.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LamazonDb");

        // Database
        services.AddDbContext<LamazonDbContext>(options => options.UseSqlServer(connectionString));



        return services;
    }
}
