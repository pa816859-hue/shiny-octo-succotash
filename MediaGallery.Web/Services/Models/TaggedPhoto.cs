using System;
using System.Collections.Generic;

namespace MediaGallery.Web.Services.Models;

public sealed class TaggedPhoto
{
    public TaggedPhoto(
        long photoId,
        string filePath,
        DateTime addedOn,
        IReadOnlyList<PhotoTag> tags,
        long? channelId,
        long? messageId,
        DateTime? sentDate,
        string? messageText,
        long? userId,
        string? username,
        string? firstName,
        string? lastName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }

        PhotoId = photoId;
        FilePath = filePath;
        AddedOn = addedOn;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        ChannelId = channelId;
        MessageId = messageId;
        SentDate = sentDate;
        MessageText = messageText;
        UserId = userId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
    }

    public long PhotoId { get; }

    public string FilePath { get; }

    public DateTime AddedOn { get; }

    public IReadOnlyList<PhotoTag> Tags { get; }

    public long? ChannelId { get; }

    public long? MessageId { get; }

    public DateTime? SentDate { get; }

    public string? MessageText { get; }

    public long? UserId { get; }

    public string? Username { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public string DisplayName => DisplayNameFormatter.Build(UserId, Username, FirstName, LastName);
}
