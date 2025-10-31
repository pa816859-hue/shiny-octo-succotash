using System;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class UserProfileViewModel
{
    public UserProfileViewModel(UserProfile user, IReadOnlyList<RecentMessage> messages, PaginationMetadata pagination)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public UserProfile User { get; }

    public IReadOnlyList<RecentMessage> Messages { get; }

    public PaginationMetadata Pagination { get; }
}
