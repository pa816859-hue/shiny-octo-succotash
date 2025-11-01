using System;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class MessageQueryViewModel
{
    public MessageQueryViewModel(
        int page,
        int pageSize,
        long? channelId,
        long? userId,
        bool mediaOnly,
        MessageSortOrder sortOrder)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        Page = page;
        PageSize = pageSize;
        ChannelId = channelId;
        UserId = userId;
        MediaOnly = mediaOnly;
        SortOrder = sortOrder;
    }

    public int Page { get; }

    public int PageSize { get; }

    public long? ChannelId { get; }

    public long? UserId { get; }

    public bool MediaOnly { get; }

    public MessageSortOrder SortOrder { get; }
}
