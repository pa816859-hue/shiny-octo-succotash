using System;
using System.Collections.Generic;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class RecentMessagesViewModel
{
    public RecentMessagesViewModel(IReadOnlyList<RecentMessage> messages, PaginationMetadata pagination)
    {
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public IReadOnlyList<RecentMessage> Messages { get; }

    public PaginationMetadata Pagination { get; }
}
