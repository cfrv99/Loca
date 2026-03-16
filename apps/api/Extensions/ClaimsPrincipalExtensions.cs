using System.Security.Claims;

namespace Loca.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return sub is not null ? Guid.Parse(sub) : throw new UnauthorizedAccessException("User ID not found in token");
    }

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirst("email")?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? string.Empty;

    public static string GetDisplayName(this ClaimsPrincipal user)
        => user.FindFirst("name")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? "Unknown";

    public static string GetGender(this ClaimsPrincipal user)
        => user.FindFirst("gender")?.Value ?? "unknown";
}
