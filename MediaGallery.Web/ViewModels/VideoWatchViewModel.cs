namespace MediaGallery.Web.ViewModels;

public sealed class VideoWatchViewModel
{
    public VideoWatchViewModel(VideoPlaybackViewModel? currentVideo)
    {
        CurrentVideo = currentVideo;
    }

    public VideoPlaybackViewModel? CurrentVideo { get; }

    public bool HasVideo => CurrentVideo is not null;
}
