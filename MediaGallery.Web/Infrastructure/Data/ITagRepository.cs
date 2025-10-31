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
}
