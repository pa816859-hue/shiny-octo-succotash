using System;
using System.Collections.Generic;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class TagQueryResultViewModel
{
    public TagQueryResultViewModel(
        IReadOnlyList<string> includes,
        IReadOnlyList<string> excludes,
        PagedResult<TaggedPhoto> photos)
    {
        Includes = includes ?? throw new ArgumentNullException(nameof(includes));
        Excludes = excludes ?? throw new ArgumentNullException(nameof(excludes));
        Photos = photos ?? throw new ArgumentNullException(nameof(photos));
    }

    public IReadOnlyList<string> Includes { get; }

    public IReadOnlyList<string> Excludes { get; }

    public PagedResult<TaggedPhoto> Photos { get; }
}
