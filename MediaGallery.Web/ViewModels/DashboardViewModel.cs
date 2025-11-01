using System;
using System.Collections.Generic;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class DashboardViewModel
{
    public DashboardViewModel(
        DashboardSummary summary,
        IReadOnlyList<RecentMessage> recentMessages,
        IReadOnlyList<PhotoTagSummary> topTags,
        IReadOnlyList<UserSummary> recentUsers)
    {
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        RecentMessages = recentMessages ?? throw new ArgumentNullException(nameof(recentMessages));
        TopTags = topTags ?? throw new ArgumentNullException(nameof(topTags));
        RecentUsers = recentUsers ?? throw new ArgumentNullException(nameof(recentUsers));
    }

    public DashboardSummary Summary { get; }

    public IReadOnlyList<RecentMessage> RecentMessages { get; }

    public IReadOnlyList<PhotoTagSummary> TopTags { get; }

    public IReadOnlyList<UserSummary> RecentUsers { get; }
}
