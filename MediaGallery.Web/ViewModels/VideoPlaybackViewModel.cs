using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaGallery.Web.ViewModels;

public sealed class VideoPlaybackViewModel
{
    private readonly List<VideoContributorViewModel> _contributors;

    public VideoPlaybackViewModel(long videoId, string sourceUrl, DateTime addedOn, bool isLiked, IEnumerable<VideoContributorViewModel>? contributors = null)
    {
        VideoId = videoId;
        SourceUrl = sourceUrl;
        AddedOn = addedOn;
        IsLiked = isLiked;
        _contributors = contributors?.ToList() ?? new List<VideoContributorViewModel>();
    }

    public long VideoId { get; }

    public string SourceUrl { get; }

    public DateTime AddedOn { get; }

    public bool IsLiked { get; }

    public IReadOnlyList<VideoContributorViewModel> Contributors => _contributors;
}
