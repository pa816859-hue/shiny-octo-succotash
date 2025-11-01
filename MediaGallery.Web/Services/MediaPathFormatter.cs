using System;
using System.IO;

namespace MediaGallery.Web.Services;

public static class MediaPathFormatter
{
    public static string? ToRelativeWebPath(string? path, string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var normalizedPath = NormalizeSeparators(path.Trim());

        if (!string.IsNullOrWhiteSpace(rootDirectory))
        {
            var normalizedRoot = NormalizeSeparators(rootDirectory).TrimEnd('/');

            if (normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring(normalizedRoot.Length);
            }
            else
            {
                var rootFullPath = Path.GetFullPath(rootDirectory);
                var candidateFullPath = Path.IsPathRooted(path)
                    ? Path.GetFullPath(path)
                    : Path.GetFullPath(Path.Combine(rootFullPath, path.TrimStart('/', '\\')));

                var relative = Path.GetRelativePath(rootFullPath, candidateFullPath);
                if (!relative.StartsWith("..", StringComparison.Ordinal))
                {
                    normalizedPath = NormalizeSeparators(relative);
                }
            }
        }

        normalizedPath = normalizedPath.TrimStart('/');

        while (normalizedPath.StartsWith("./", StringComparison.Ordinal))
        {
            normalizedPath = normalizedPath.Substring(2);
        }

        if (normalizedPath == ".")
        {
            return string.Empty;
        }

        return normalizedPath;
    }

    private static string NormalizeSeparators(string value)
    {
        var normalized = value.Replace('\\', '/');
        while (normalized.Contains("//", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("//", "/");
        }

        return normalized;
    }
}
