using System;

namespace MediaGallery.Web.ViewModels;

public sealed class UserProfilePageViewModel
{
    public UserProfilePageViewModel(UserProfileViewModel profile, MessageQueryViewModel query)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Query = query ?? throw new ArgumentNullException(nameof(query));
    }

    public UserProfileViewModel Profile { get; }

    public MessageQueryViewModel Query { get; }
}
