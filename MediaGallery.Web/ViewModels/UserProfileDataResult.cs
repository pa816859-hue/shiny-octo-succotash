using System;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class UserProfileDataResult
{
    public UserProfileDataResult(UserProfile user, PagedResult<RecentMessage> messages)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    public UserProfile User { get; }

    public PagedResult<RecentMessage> Messages { get; }
}
