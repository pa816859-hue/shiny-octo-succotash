using System;

namespace MediaGallery.Web.ViewModels;

public sealed class PhotoDisplayViewModel
{
    public PhotoDisplayViewModel(long photoId, string sourceUrl, DateTime addedOn, bool isLiked)
    {
        PhotoId = photoId;
        SourceUrl = sourceUrl ?? throw new ArgumentNullException(nameof(sourceUrl));
        AddedOn = addedOn;
        IsLiked = isLiked;
    }

    public long PhotoId { get; }

    public string SourceUrl { get; }

    public DateTime AddedOn { get; }

    public bool IsLiked { get; }
}
