using System;

namespace MediaGallery.Web.Services.Models;

public sealed class UserSummary
{
    public UserSummary(long userId, string? username, string? firstName, string? lastName, DateTime lastUpdate)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        UserId = userId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        LastUpdate = lastUpdate;
    }

    public long UserId { get; }

    public string? Username { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public DateTime LastUpdate { get; }

    public string DisplayName => DisplayNameFormatter.Build(UserId, Username, FirstName, LastName);
}
