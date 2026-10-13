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

    /// <summary>The id of the logged-in user. Only call it where [Authorize] guarantees a logged-in user.</summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId)
            ? userId
            : throw new InvalidOperationException("The user is not logged in.");
    }
}
