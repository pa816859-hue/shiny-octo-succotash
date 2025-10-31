namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record MessageDto(
    long ChannelId,
    long MessageId,
    long UserId,
    DateTime SentDate,
    string? MessageText,
    long? PhotoId,
    long? VideoId);
