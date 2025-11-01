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

public class PhotoService : IPhotoService
{
    private const int DefaultBatchSize = 50;
    private const int MaxRandomFetchAttempts = 5;

    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoStateStore _stateStore;
    private readonly IOptionsMonitor<MediaOptions> _mediaOptions;

    public PhotoService(
        IPhotoRepository photoRepository,
        IPhotoStateStore stateStore,
        IOptionsMonitor<MediaOptions> mediaOptions)
    {
        _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _mediaOptions = mediaOptions ?? throw new ArgumentNullException(nameof(mediaOptions));
    }

    public async Task<PhotoDisplayModel?> GetNextPhotoAsync(CancellationToken cancellationToken = default)
    {
        var mediaRoot = _mediaOptions.CurrentValue.RootDirectory;
        if (string.IsNullOrWhiteSpace(mediaRoot))
        {
            return null;
        }

        var viewedIds = await _stateStore.GetViewedPhotoIdsAsync(cancellationToken).ConfigureAwait(false);
        var likedIds = await _stateStore.GetLikedPhotoIdsAsync(cancellationToken).ConfigureAwait(false);

        var attemptedIds = new HashSet<long>();

        for (var attempt = 0; attempt < MaxRandomFetchAttempts; attempt++)
        {
            var randomPhotos = await _photoRepository
                .GetRandomPhotosAsync(DefaultBatchSize, cancellationToken)
                .ConfigureAwait(false);

            if (randomPhotos.Count == 0)
            {
                continue;
            }

            var candidates = new List<PhotoDisplayModel>(randomPhotos.Count);
            foreach (var photo in randomPhotos)
            {
                if (!attemptedIds.Add(photo.PhotoId))
                {
                    continue;
                }

                var model = CreateDisplayModel(photo, mediaRoot, likedIds);
                if (model is null)
                {
                    continue;
                }

                candidates.Add(model);
            }

            if (candidates.Count == 0)
            {
                continue;
            }

            var unviewed = candidates.FirstOrDefault(candidate => !viewedIds.Contains(candidate.PhotoId));
            if (unviewed is null)
            {
                continue;
            }

            await _stateStore.AddViewedPhotoIdAsync(unviewed.PhotoId, cancellationToken).ConfigureAwait(false);
            return unviewed;
        }

        return null;
    }

    public async Task<PhotoDisplayModel?> SkipAsync(long photoId, CancellationToken cancellationToken = default)
    {
        if (photoId > 0)
        {
            await _stateStore.AddViewedPhotoIdAsync(photoId, cancellationToken).ConfigureAwait(false);
        }

        return await GetNextPhotoAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<PhotoDisplayModel?> LikeAsync(long photoId, CancellationToken cancellationToken = default)
    {
        if (photoId <= 0)
        {
            return await GetNextPhotoAsync(cancellationToken).ConfigureAwait(false);
        }

        await _stateStore.AddViewedPhotoIdAsync(photoId, cancellationToken).ConfigureAwait(false);
        await _stateStore.AddLikedPhotoIdAsync(photoId, cancellationToken).ConfigureAwait(false);

        return await GetNextPhotoAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> RemoveLikeAsync(long photoId, CancellationToken cancellationToken = default)
    {
        if (photoId <= 0)
        {
            return Task.FromResult(false);
        }

        return _stateStore.RemoveLikedPhotoIdAsync(photoId, cancellationToken);
    }

    public async Task<IReadOnlyList<PhotoDisplayModel>> GetLikedPhotosAsync(CancellationToken cancellationToken = default)
    {
        var mediaRoot = _mediaOptions.CurrentValue.RootDirectory;
        if (string.IsNullOrWhiteSpace(mediaRoot))
        {
            return Array.Empty<PhotoDisplayModel>();
        }

        var likedIds = await _stateStore.GetLikedPhotoIdsAsync(cancellationToken).ConfigureAwait(false);
        if (likedIds.Count == 0)
        {
            return Array.Empty<PhotoDisplayModel>();
        }

        var photos = await _photoRepository
            .GetPhotosByIdsAsync(likedIds, cancellationToken)
            .ConfigureAwait(false);

        var likedSet = likedIds;
        return photos
            .Select(photo => CreateDisplayModel(photo, mediaRoot, likedSet))
            .Where(model => model is not null)
            .Select(model => model!)
            .OrderByDescending(model => model.AddedOn)
            .ThenByDescending(model => model.PhotoId)
            .ToList();
    }

    private static PhotoDisplayModel? CreateDisplayModel(PhotoDto dto, string mediaRoot, HashSet<long> likedIds)
    {
        var relativePath = MediaPathFormatter.ToRelativeWebPath(dto.FilePath, mediaRoot);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var normalizedPath = relativePath.TrimStart('/', '\\');
        var sourceUrl = "/media/" + normalizedPath.Replace('\\', '/');
        var isLiked = likedIds.Contains(dto.PhotoId);

        return new PhotoDisplayModel(dto.PhotoId, sourceUrl, dto.AddedOn, isLiked);
    }
}
