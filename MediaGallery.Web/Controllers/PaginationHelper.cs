using System;

namespace MediaGallery.Web.Controllers;

internal static class PaginationHelper
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSizeLimit = 500;

    public static bool TryValidate(int pageNumber, int pageSize, out string? errorMessage)
    {
        if (pageNumber < 1)
        {
            errorMessage = "Page number must be greater than or equal to 1.";
            return false;
        }

        if (pageSize < 1)
        {
            errorMessage = "Page size must be greater than or equal to 1.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static int ClampPageSize(int pageSize, int maxPageSize = MaxPageSizeLimit)
    {
        if (maxPageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPageSize));
        }

        if (pageSize < 1)
        {
            return 1;
        }

        return pageSize > maxPageSize ? maxPageSize : pageSize;
    }
}
