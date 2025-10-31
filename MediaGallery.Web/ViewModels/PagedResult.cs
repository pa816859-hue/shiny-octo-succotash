using System;
using System.Collections.Generic;

namespace MediaGallery.Web.ViewModels;

public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, PaginationMetadata pagination)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public IReadOnlyList<T> Items { get; }

    public PaginationMetadata Pagination { get; }
}
