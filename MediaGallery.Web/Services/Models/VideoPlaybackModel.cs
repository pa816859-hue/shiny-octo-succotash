using System;

namespace MediaGallery.Web.Services.Models;

public sealed class VideoPlaybackModel
{
    public VideoPlaybackModel(long videoId, string sourceUrl, DateTime addedOn, bool isLiked)
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
