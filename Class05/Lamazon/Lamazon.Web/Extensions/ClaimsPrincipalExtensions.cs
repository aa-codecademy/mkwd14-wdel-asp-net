using System.Security.Claims;
using Lamazon.Domain.Constants;

namespace Lamazon.Web.Extensions;

/// <summary>
/// Shortcuts for reading the logged-in user's claims (User in controllers and views is a ClaimsPrincipal).
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Admin);
    }

    public static string DisplayName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }

}
