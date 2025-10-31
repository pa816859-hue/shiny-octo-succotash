using System;
using System.Collections.Generic;

namespace MediaGallery.Web.Services.Models;

public sealed class UserProfile
{
    public UserProfile(
        long userId,
        string? username,
        string? firstName,
        string? lastName,
        DateTime lastUpdate,
        IReadOnlyList<UserTag> tags)
    {
        UserId = userId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        LastUpdate = lastUpdate;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
    }

    public long UserId { get; }

    public string? Username { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public DateTime LastUpdate { get; }

    public IReadOnlyList<UserTag> Tags { get; }

    public string DisplayName => DisplayNameFormatter.Build(UserId, Username, FirstName, LastName);
}
