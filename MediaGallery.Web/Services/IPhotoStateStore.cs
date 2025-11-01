using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaGallery.Web.Services;

public interface IPhotoStateStore
{
    Task<HashSet<long>> GetViewedPhotoIdsAsync(CancellationToken cancellationToken = default);

    Task<HashSet<long>> GetLikedPhotoIdsAsync(CancellationToken cancellationToken = default);

    Task AddViewedPhotoIdAsync(long photoId, CancellationToken cancellationToken = default);

    Task AddLikedPhotoIdAsync(long photoId, CancellationToken cancellationToken = default);

    Task<bool> RemoveLikedPhotoIdAsync(long photoId, CancellationToken cancellationToken = default);
}
