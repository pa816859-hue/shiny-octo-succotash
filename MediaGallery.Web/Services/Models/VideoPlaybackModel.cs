using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaGallery.Web.Services.Models;

public sealed class VideoPlaybackModel
{
    private readonly List<VideoContributor> _contributors;

    public VideoPlaybackModel(long videoId, string sourceUrl, DateTime addedOn, bool isLiked, IEnumerable<VideoContributor>? contributors = null)
    {
        VideoId = videoId;
        SourceUrl = sourceUrl;
        AddedOn = addedOn;
        IsLiked = isLiked;
        _contributors = contributors?.ToList() ?? new List<VideoContributor>();
    }

    public long VideoId { get; }

    public string SourceUrl { get; }

    public DateTime AddedOn { get; }

    public bool IsLiked { get; }

    public IReadOnlyList<VideoContributor> Contributors => _contributors;
}
