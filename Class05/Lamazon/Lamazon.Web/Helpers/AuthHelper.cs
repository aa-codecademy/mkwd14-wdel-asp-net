using System.Security.Claims;
using Lamazon.ViewModels.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Lamazon.Web.Helpers;

public static class AuthHelper
{
    /// <summary>
    /// Logs the user in: the claims (facts about the user) are encrypted into the login cookie.
    /// From now on, User in every controller and view is this user.
    /// </summary>
    public static async Task SignInUserAsync(HttpContext httpContext, UserViewModel user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            // [Authorize(Roles = Roles.Admin)] and User.IsInRole(...) read this claim
            new(ClaimTypes.Role, user.RoleKey),
        ];

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            // Keep the cookie after the browser is closed (until it expires)
            IsPersistent = true
        };

        await httpContext.SignInAsync(claimsPrinciple, authProperties);
    }

    /// <summary>Logs the user out: deletes the login cookie.</summary>
    public static async Task SignOutUserAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
