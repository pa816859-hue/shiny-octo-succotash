using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IPhotoRepository
{
    Task<IReadOnlyList<PhotoDto>> GetPhotosAsync(
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default);
}
