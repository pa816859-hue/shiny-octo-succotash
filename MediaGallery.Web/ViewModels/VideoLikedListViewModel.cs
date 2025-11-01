using System.Collections.Generic;
using System.Linq;

namespace MediaGallery.Web.ViewModels;

public sealed class VideoLikedListViewModel
{
    public VideoLikedListViewModel(IEnumerable<VideoPlaybackViewModel> videos)
    {
        Videos = videos?.ToList() ?? new List<VideoPlaybackViewModel>();
    }

    public IReadOnlyList<VideoPlaybackViewModel> Videos { get; }

    public int TotalVideos => Videos.Count;

    public bool HasVideos => TotalVideos > 0;
}
