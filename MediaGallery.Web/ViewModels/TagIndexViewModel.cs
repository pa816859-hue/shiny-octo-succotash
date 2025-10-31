using System;
using System.Collections.Generic;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class TagIndexViewModel
{
    public TagIndexViewModel(IReadOnlyList<PhotoTagSummary> tags, PaginationMetadata pagination)
    {
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public IReadOnlyList<PhotoTagSummary> Tags { get; }

    public PaginationMetadata Pagination { get; }
}
