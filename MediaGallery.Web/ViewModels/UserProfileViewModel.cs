using System;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class UserProfileViewModel
{
    public UserProfileViewModel(UserProfile user, PaginationMetadata pagination)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public UserProfile User { get; }

    public PaginationMetadata Pagination { get; }
}
