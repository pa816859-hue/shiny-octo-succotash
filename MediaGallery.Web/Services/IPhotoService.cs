using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.Services;

public interface IPhotoService
{
    Task<PhotoDisplayModel?> GetNextPhotoAsync(CancellationToken cancellationToken = default);

    Task<PhotoDisplayModel?> SkipAsync(long photoId, CancellationToken cancellationToken = default);

    Task<PhotoDisplayModel?> LikeAsync(long photoId, CancellationToken cancellationToken = default);

    Task<bool> RemoveLikeAsync(long photoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PhotoDisplayModel>> GetLikedPhotosAsync(CancellationToken cancellationToken = default);
}
