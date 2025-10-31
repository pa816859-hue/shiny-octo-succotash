using System;

namespace MediaGallery.Web.Services.Models;

public sealed class RecentMessage
{
    public RecentMessage(
        long channelId,
        long messageId,
        long userId,
        string? username,
        string? firstName,
        string? lastName,
        DateTime sentDate,
        string? messageText,
        long? photoId,
        long? videoId)
    {
        ChannelId = channelId;
        MessageId = messageId;
        UserId = userId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        SentDate = sentDate;
        MessageText = messageText;
        PhotoId = photoId;
        VideoId = videoId;
    }

    public long ChannelId { get; }

    public long MessageId { get; }

    public long UserId { get; }

    public string? Username { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public DateTime SentDate { get; }

    public string? MessageText { get; }

    public long? PhotoId { get; }

    public long? VideoId { get; }

    public string DisplayName => DisplayNameFormatter.Build(UserId, Username, FirstName, LastName);
}
