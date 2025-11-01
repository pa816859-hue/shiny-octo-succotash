using System;

namespace MediaGallery.Web.ViewModels;

public sealed class VideoPlaybackViewModel
{
    public VideoPlaybackViewModel(long videoId, string sourceUrl, DateTime addedOn, bool isLiked)
    {
        VideoId = videoId;
        SourceUrl = sourceUrl;
        AddedOn = addedOn;
        IsLiked = isLiked;
    }

    public long VideoId { get; }

    public string SourceUrl { get; }

    public DateTime AddedOn { get; }

    public bool IsLiked { get; }
}
