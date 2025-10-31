using System;

namespace MediaGallery.Web.ViewModels;

public sealed class PaginationMetadata
{
    public PaginationMetadata(int pageNumber, int pageSize, bool hasNextPage, bool hasPreviousPage)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
        HasNextPage = hasNextPage;
        HasPreviousPage = hasPreviousPage;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public bool HasNextPage { get; }

    public bool HasPreviousPage { get; }
}
