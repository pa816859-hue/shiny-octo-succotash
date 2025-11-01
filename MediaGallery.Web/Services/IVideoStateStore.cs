using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaGallery.Web.Services;

public interface IVideoStateStore
{
    Task<HashSet<long>> GetWatchedVideoIdsAsync(CancellationToken cancellationToken = default);

    Task<HashSet<long>> GetLikedVideoIdsAsync(CancellationToken cancellationToken = default);

    Task AddWatchedVideoIdAsync(long videoId, CancellationToken cancellationToken = default);

    Task AddLikedVideoIdAsync(long videoId, CancellationToken cancellationToken = default);

    Task<bool> RemoveLikedVideoIdAsync(long videoId, CancellationToken cancellationToken = default);
}
