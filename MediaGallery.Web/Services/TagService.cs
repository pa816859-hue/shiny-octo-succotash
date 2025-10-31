using System;
using System.Linq;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using MediaGallery.Web.Services.Mapping;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;

    public TagService(ITagRepository tagRepository, IUserRepository userRepository)
    {
        _tagRepository = tagRepository;
        _userRepository = userRepository;
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

        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);
        return trimmedDetails.ToTagDetailViewModel(tag, pagination);
    }
}
