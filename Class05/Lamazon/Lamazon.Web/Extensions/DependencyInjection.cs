using Lamazon.Web.Constants;
using Lamazon.Web.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Lamazon.Web.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// MVC with our exception filter, and login with a cookie.
    /// </summary>
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        services.AddControllersWithViews(options =>
        {
            // Turns NotFoundException into a 404 page and BusinessRuleException into a message (see Filters)
            options.Filters.Add<AppExceptionFilter>();

            // FluentValidation checks our forms. Without this line MVC would ALSO treat every
            // non-nullable string as [Required] and add its own "The X field is required." errors.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        });

        // Cookie authentication: after login, the browser keeps an encrypted cookie with the user's claims and sends it with every request
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Users/Login";  // not logged in => go here
                options.AccessDeniedPath = "/Users/AccessDenied"; // logged in, but not allowed (e.g. not an admin)
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true; // every visit extends the hour
                options.Cookie.Name = Cookies.Auth;
                options.Cookie.HttpOnly = true; // JavaScript can't read it
                options.Cookie.SameSite = SameSiteMode.Lax; 
            });

        services.AddAuthentication();

        return services;
    }
}
