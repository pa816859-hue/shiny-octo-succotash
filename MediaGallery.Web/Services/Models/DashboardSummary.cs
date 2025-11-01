using System;

namespace MediaGallery.Web.Services.Models;

public sealed class DashboardSummary
{
    public DashboardSummary(
        long totalMessages,
        long totalPhotos,
        long totalVideos,
        long activeChannels,
        long totalUsers,
        DateTime? lastMessageSentAt)
    {
        if (totalMessages < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalMessages));
        }

        if (totalPhotos < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalPhotos));
        }

        if (totalVideos < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalVideos));
        }

        if (activeChannels < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeChannels));
        }

        if (totalUsers < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalUsers));
        }

        TotalMessages = totalMessages;
        TotalPhotos = totalPhotos;
        TotalVideos = totalVideos;
        ActiveChannels = activeChannels;
        TotalUsers = totalUsers;
        LastMessageSentAt = lastMessageSentAt;
    }

    public long TotalMessages { get; }

    public long TotalPhotos { get; }

    public long TotalVideos { get; }

    public long ActiveChannels { get; }

    public long TotalUsers { get; }

    public DateTime? LastMessageSentAt { get; }
}
