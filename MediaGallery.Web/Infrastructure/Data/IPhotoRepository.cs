using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IPhotoRepository
{
    Task<IReadOnlyList<PhotoDto>> GetPhotosAsync(
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PhotoDto>> GetRandomPhotosAsync(
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PhotoDto>> GetPhotosByIdsAsync(
        IEnumerable<long> photoIds,
        CancellationToken cancellationToken = default);
}
