using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using MediaGallery.Web.Services.Mapping;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public class TagService : ITagService
{
    private const int QueryTagLimit = 12;

    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMediaFileProvider _mediaFileProvider;

    public TagService(ITagRepository tagRepository, IUserRepository userRepository, IMediaFileProvider mediaFileProvider)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mediaFileProvider = mediaFileProvider ?? throw new ArgumentNullException(nameof(mediaFileProvider));
    }

    public async Task<TagIndexViewModel> GetTagIndexAsync(
        int pageNumber,
        int pageSize,
        long? userId,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var offset = (pageNumber - 1) * pageSize;
        var fetchLimit = checked(pageSize + 1);

        var summaries = await _tagRepository
            .GetTagSummariesAsync(offset, fetchLimit, userId, cancellationToken)
            .ConfigureAwait(false);

        var hasNextPage = summaries.Count > pageSize;
        var trimmedSummaries = hasNextPage ? summaries.Take(pageSize).ToList() : summaries.ToList();

        UserDto? user = null;
        if (userId.HasValue)
        {
            user = await _userRepository.GetUserByIdAsync(userId.Value, cancellationToken).ConfigureAwait(false);
        }

        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);
        return trimmedSummaries.ToTagIndexViewModel(user, pagination);
    }

    public async Task<TagDetailViewModel> GetTagDetailAsync(
        string tag,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be null or whitespace.", nameof(tag));
        }

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var offset = (pageNumber - 1) * pageSize;
        var fetchLimit = checked(pageSize + 1);

        var details = await _tagRepository
            .GetTagDetailsAsync(tag, offset, fetchLimit, cancellationToken)
            .ConfigureAwait(false);

        var hasNextPage = details.Count > pageSize;
        var trimmedDetails = hasNextPage ? details.Take(pageSize).ToList() : details.ToList();
        var normalizedDetails = trimmedDetails
            .Select(detail => detail with
            {
                PhotoPath = MediaPathFormatter.ToRelativeWebPath(detail.PhotoPath, _mediaFileProvider.RootDirectory)
            })
            .ToList();

        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);
        return normalizedDetails.ToTagDetailViewModel(tag, pagination);
    }

    public async Task<TagQueryResultViewModel> QueryTagsAsync(
        IEnumerable<string> includeTags,
        IEnumerable<string> excludeTags,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var normalizedIncludes = NormalizeTags(includeTags);
        var normalizedExcludes = NormalizeExcludes(excludeTags, normalizedIncludes);

        var offset = (pageNumber - 1) * pageSize;
        var fetchLimit = checked(pageSize + 1);

        var details = await _tagRepository
            .QueryTagDetailsAsync(normalizedIncludes, normalizedExcludes, offset, fetchLimit, cancellationToken)
            .ConfigureAwait(false);

        var hasNextPage = details.Count > pageSize;
        var trimmedDetails = hasNextPage ? details.Take(pageSize).ToList() : details.ToList();

        var tagTasks = trimmedDetails
            .Select(detail => LoadPhotoTagsAsync(detail.PhotoId, cancellationToken))
            .ToList();

        var tagResults = await Task.WhenAll(tagTasks).ConfigureAwait(false);

        var taggedPhotos = new List<TaggedPhoto>(trimmedDetails.Count);
        for (var index = 0; index < trimmedDetails.Count; index++)
        {
            var detail = trimmedDetails[index];
            var tags = tagResults[index];
            var relativePath = MediaPathFormatter.ToRelativeWebPath(detail.PhotoPath, _mediaFileProvider.RootDirectory);

            taggedPhotos.Add(new TaggedPhoto(
                detail.PhotoId,
                relativePath,
                detail.PhotoAddedOn,
                tags,
                detail.ChannelId,
                detail.MessageId,
                detail.SentDate,
                detail.MessageText,
                detail.UserId,
                detail.Username,
                detail.FirstName,
                detail.LastName));
        }

        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);
        var photos = new PagedResult<TaggedPhoto>(taggedPhotos, pagination);
        return new TagQueryResultViewModel(normalizedIncludes, normalizedExcludes, photos);
    }

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string> tags)
    {
        var normalized = new List<string>();
        if (tags is null)
        {
            return normalized;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            var trimmed = tag.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (seen.Add(trimmed))
            {
                normalized.Add(trimmed);
            }
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeExcludes(IEnumerable<string> excludes, IReadOnlyCollection<string> includes)
    {
        var normalized = NormalizeTags(excludes);
        if (normalized.Count == 0 || includes.Count == 0)
        {
            return normalized;
        }

        var includeSet = new HashSet<string>(includes, StringComparer.OrdinalIgnoreCase);
        return normalized
            .Where(tag => !includeSet.Contains(tag))
            .ToList();
    }

    private async Task<IReadOnlyList<PhotoTag>> LoadPhotoTagsAsync(long photoId, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository
            .GetPhotoTagsAsync(photoId, 0, QueryTagLimit, cancellationToken)
            .ConfigureAwait(false);

        return tags
            .Select(tag => new PhotoTag(tag.Tag, tag.Score))
            .ToList();
    }
}
