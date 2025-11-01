using System;

namespace MediaGallery.Web.ViewModels;

public sealed class VideoContributorViewModel
{
    public VideoContributorViewModel(long userId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        UserId = userId;
        DisplayName = displayName;
    }

    public long UserId { get; }

    public string DisplayName { get; }
}
