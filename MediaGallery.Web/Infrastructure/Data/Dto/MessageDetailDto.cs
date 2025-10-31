namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record MessageDetailDto(
    long ChannelId,
    long MessageId,
    long UserId,
    string? Username,
    string? FirstName,
    string? LastName,
    DateTime SentDate,
    string? MessageText,
    long? PhotoId,
    string? PhotoPath,
    long? VideoId,
    string? VideoPath);
