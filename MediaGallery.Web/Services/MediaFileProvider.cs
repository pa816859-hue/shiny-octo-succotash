using System.IO;
using MediaGallery.Web.Configurations;
using Microsoft.Extensions.Options;

namespace MediaGallery.Web.Services;

public class MediaFileProvider : IMediaFileProvider
{
    private readonly IOptionsMonitor<MediaOptions> _options;

    public MediaFileProvider(IOptionsMonitor<MediaOptions> options)
    {
        _options = options;
    }

    public string RootDirectory => _options.CurrentValue.RootDirectory;

    public string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("A relative path must be provided.", nameof(relativePath));
        }

        var root = RootDirectory;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("The media root directory has not been configured.");
        }

        return Path.GetFullPath(Path.Combine(root, relativePath));
    }
}
