using System.Globalization;
using System.Linq;

namespace MediaGallery.Web.Services.Models;

internal static class DisplayNameFormatter
{
    public static string Build(long? userId, string? username, string? firstName, string? lastName)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            return username!;
        }

        var parts = new[] { firstName, lastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim())
            .ToArray();

        if (parts.Length > 0)
        {
            return string.Join(" ", parts);
        }

        return userId.HasValue
            ? userId.Value.ToString(CultureInfo.InvariantCulture)
            : "Unknown User";
    }
}
