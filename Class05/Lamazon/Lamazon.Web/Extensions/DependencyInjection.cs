using Lamazon.Web.Filters;

namespace Lamazon.Web.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add<AppExceptionFilter>();
        });

        return services;
    }
}
