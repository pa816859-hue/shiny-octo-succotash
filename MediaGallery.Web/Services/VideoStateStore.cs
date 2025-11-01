using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace MediaGallery.Web.Services;

public class VideoStateStore : IVideoStateStore
{
    private const string WatchedFileName = "watched-videos.txt";
    private const string LikedFileName = "liked-videos.txt";

    private readonly string _dataDirectory;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public VideoStateStore(IWebHostEnvironment environment)
    {
        if (environment is null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        _dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
    }

    public Task<HashSet<long>> GetWatchedVideoIdsAsync(CancellationToken cancellationToken = default)
        => ReadIdsAsync(GetWatchedFilePath(), cancellationToken);

    public Task<HashSet<long>> GetLikedVideoIdsAsync(CancellationToken cancellationToken = default)
        => ReadIdsAsync(GetLikedFilePath(), cancellationToken);

    public Task AddWatchedVideoIdAsync(long videoId, CancellationToken cancellationToken = default)
        => AddIdAsync(GetWatchedFilePath(), videoId, cancellationToken);

    public Task AddLikedVideoIdAsync(long videoId, CancellationToken cancellationToken = default)
        => AddIdAsync(GetLikedFilePath(), videoId, cancellationToken);

    public Task<bool> RemoveLikedVideoIdAsync(long videoId, CancellationToken cancellationToken = default)
        => RemoveIdAsync(GetLikedFilePath(), videoId, cancellationToken);

    private async Task<HashSet<long>> ReadIdsAsync(string filePath, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return new HashSet<long>();
            }

            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
            var ids = new HashSet<long>(lines.Length);

            foreach (var line in lines)
            {
                if (long.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
                {
                    ids.Add(id);
                }
            }

            return ids;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task AddIdAsync(string filePath, long videoId, CancellationToken cancellationToken)
    {
        if (videoId <= 0)
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(_dataDirectory);

            var existing = await ReadIdsInternalAsync(filePath, cancellationToken).ConfigureAwait(false);
            if (!existing.Add(videoId))
            {
                return;
            }

            var line = videoId.ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            await File.AppendAllTextAsync(filePath, line, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<bool> RemoveIdAsync(string filePath, long videoId, CancellationToken cancellationToken)
    {
        if (videoId <= 0)
        {
            return false;
        }

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
            if (lines.Length == 0)
            {
                return false;
            }

            var updated = new List<string>(lines.Length);
            var removed = false;

            foreach (var line in lines)
            {
                if (!long.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
                {
                    continue;
                }

                if (!removed && parsed == videoId)
                {
                    removed = true;
                    continue;
                }

                updated.Add(parsed.ToString(CultureInfo.InvariantCulture));
            }

            if (!removed)
            {
                return false;
            }

            if (updated.Count == 0)
            {
                File.Delete(filePath);
            }
            else
            {
                await File.WriteAllLinesAsync(filePath, updated, cancellationToken).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<HashSet<long>> ReadIdsInternalAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return new HashSet<long>();
        }

        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<long>(lines.Length);

        foreach (var line in lines)
        {
            if (long.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    private string GetWatchedFilePath() => Path.Combine(_dataDirectory, WatchedFileName);

    private string GetLikedFilePath() => Path.Combine(_dataDirectory, LikedFileName);
}
