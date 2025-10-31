using System;

namespace MediaGallery.Web.Services.Models;

public sealed class PhotoTagSummary
{
    public PhotoTagSummary(
        string tag,
        int photoCount,
        long? userId,
        string? username,
        string? firstName,
        string? lastName)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be null or whitespace.", nameof(tag));
        }

        if (photoCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(photoCount));
        }

        Tag = tag;
        PhotoCount = photoCount;
        UserId = userId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
    }

    public string Tag { get; }

    public int PhotoCount { get; }

    public long? UserId { get; }

    public string? Username { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public string DisplayName => DisplayNameFormatter.Build(UserId, Username, FirstName, LastName);
}
