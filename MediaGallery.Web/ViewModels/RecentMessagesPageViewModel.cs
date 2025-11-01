using System;

namespace MediaGallery.Web.ViewModels;

public sealed class RecentMessagesPageViewModel
{
    public RecentMessagesPageViewModel(RecentMessagesViewModel feed, MessageQueryViewModel query)
    {
        Feed = feed ?? throw new ArgumentNullException(nameof(feed));
        Query = query ?? throw new ArgumentNullException(nameof(query));
    }

    public RecentMessagesViewModel Feed { get; }

    public MessageQueryViewModel Query { get; }
}
