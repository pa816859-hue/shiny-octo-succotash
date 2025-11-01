using System.Collections.Generic;
using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface ITagRepository
{
    Task<IReadOnlyList<PhotoTagDto>> GetPhotoTagsAsync(
        long photoId,
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserTagDto>> GetUserTagsAsync(
        long userId,
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagSummaryDto>> GetTagSummariesAsync(
        int offset,
        int limit,
        long? userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagDetailDto>> GetTagDetailsAsync(
        string tag,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagDetailDto>> QueryTagDetailsAsync(
        IReadOnlyCollection<string> includeTags,
        IReadOnlyCollection<string> excludeTags,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
}
