using System;
using System.Collections.Generic;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class TagDetailViewModel
{
    public TagDetailViewModel(string tag, IReadOnlyList<TaggedPhoto> photos, PaginationMetadata pagination)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be null or whitespace.", nameof(tag));
        }

        Tag = tag;
        Photos = photos ?? throw new ArgumentNullException(nameof(photos));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public string Tag { get; }

    public IReadOnlyList<TaggedPhoto> Photos { get; }

    public PaginationMetadata Pagination { get; }
}
