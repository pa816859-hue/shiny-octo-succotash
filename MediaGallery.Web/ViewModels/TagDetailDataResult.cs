using System;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class TagDetailDataResult
{
    public TagDetailDataResult(string tag, PagedResult<TaggedPhoto> photos)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be null or whitespace.", nameof(tag));
        }

        Tag = tag;
        Photos = photos ?? throw new ArgumentNullException(nameof(photos));
    }

    public string Tag { get; }

    public PagedResult<TaggedPhoto> Photos { get; }
}
