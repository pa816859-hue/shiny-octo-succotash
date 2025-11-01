using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Configurations;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using MediaGallery.Web.Services.Models;
using Microsoft.Extensions.Options;

namespace MediaGallery.Web.Services;

public class VideoService : IVideoService
{
    private const int DefaultBatchSize = 50;

    private readonly IVideoRepository _videoRepository;
    private readonly IVideoStateStore _stateStore;
    private readonly IOptionsMonitor<MediaOptions> _mediaOptions;

    public VideoService(
        IVideoRepository videoRepository,
        IVideoStateStore stateStore,
        IOptionsMonitor<MediaOptions> mediaOptions)
    {
        _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _mediaOptions = mediaOptions ?? throw new ArgumentNullException(nameof(mediaOptions));
    }

    public async Task<VideoPlaybackModel?> GetNextVideoAsync(CancellationToken cancellationToken = default)
    {
        var mediaRoot = _mediaOptions.CurrentValue.RootDirectory;
        if (string.IsNullOrWhiteSpace(mediaRoot))
        {
            return null;
        }

        var watchedIds = await _stateStore.GetWatchedVideoIdsAsync(cancellationToken).ConfigureAwait(false);
        var likedIds = await _stateStore.GetLikedVideoIdsAsync(cancellationToken).ConfigureAwait(false);

        var latestVideos = await _videoRepository
            .GetLatestVideosAsync(DefaultBatchSize, cancellationToken)
            .ConfigureAwait(false);

        var candidates = latestVideos
            .Select(video => CreatePlaybackModel(video, mediaRoot, likedIds))
            .Where(model => model is not null && !watchedIds.Contains(model.VideoId))
            .Select(model => model!)
            .ToList();

        if (candidates.Count == 0)
        {
            candidates = latestVideos
                .Select(video => CreatePlaybackModel(video, mediaRoot, likedIds))
                .Where(model => model is not null)
                .Select(model => model!)
                .ToList();
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        var selected = candidates[Random.Shared.Next(candidates.Count)];
        await _stateStore.AddWatchedVideoIdAsync(selected.VideoId, cancellationToken).ConfigureAwait(false);
        return selected;
    }

    public async Task<VideoPlaybackModel?> SkipAsync(long videoId, CancellationToken cancellationToken = default)
    {
        if (videoId > 0)
        {
            await _stateStore.AddWatchedVideoIdAsync(videoId, cancellationToken).ConfigureAwait(false);
        }

        return await GetNextVideoAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<VideoPlaybackModel?> LikeAsync(long videoId, CancellationToken cancellationToken = default)
    {
        if (videoId <= 0)
        {
            return await GetNextVideoAsync(cancellationToken).ConfigureAwait(false);
        }

        await _stateStore.AddWatchedVideoIdAsync(videoId, cancellationToken).ConfigureAwait(false);
        await _stateStore.AddLikedVideoIdAsync(videoId, cancellationToken).ConfigureAwait(false);

        return await GetNextVideoAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<VideoPlaybackModel>> GetLikedVideosAsync(CancellationToken cancellationToken = default)
    {
        var mediaRoot = _mediaOptions.CurrentValue.RootDirectory;
        if (string.IsNullOrWhiteSpace(mediaRoot))
        {
            return Array.Empty<VideoPlaybackModel>();
        }

        var likedIds = await _stateStore.GetLikedVideoIdsAsync(cancellationToken).ConfigureAwait(false);
        if (likedIds.Count == 0)
        {
            return Array.Empty<VideoPlaybackModel>();
        }

        var videos = await _videoRepository
            .GetVideosByIdsAsync(likedIds, cancellationToken)
            .ConfigureAwait(false);

        var likedSet = likedIds;
        return videos
            .Select(video => CreatePlaybackModel(video, mediaRoot, likedSet))
            .Where(model => model is not null)
            .Select(model => model!)
            .ToList();
    }

    private static VideoPlaybackModel? CreatePlaybackModel(VideoDto dto, string mediaRoot, HashSet<long> likedIds)
    {
        var relativePath = MediaPathFormatter.ToRelativeWebPath(dto.FilePath, mediaRoot);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var normalizedPath = relativePath.TrimStart('/', '\\');
        var sourceUrl = "/media/" + normalizedPath.Replace('\\', '/');
        var isLiked = likedIds.Contains(dto.VideoId);
        return new VideoPlaybackModel(dto.VideoId, sourceUrl, dto.AddedOn, isLiked);
    }
}
