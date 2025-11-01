using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.Services;

public interface IVideoService
{
    Task<VideoPlaybackModel?> GetNextVideoAsync(CancellationToken cancellationToken = default);

    Task<VideoPlaybackModel?> SkipAsync(long videoId, CancellationToken cancellationToken = default);

    Task<VideoPlaybackModel?> LikeAsync(long videoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VideoPlaybackModel>> GetLikedVideosAsync(CancellationToken cancellationToken = default);
}
