using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IVideoRepository
{
    Task<IReadOnlyList<VideoDto>> GetLatestVideosAsync(int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VideoDto>> GetVideosByIdsAsync(IEnumerable<long> videoIds, CancellationToken cancellationToken = default);
}
