namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record TagDetailDto(
    string Tag,
    long PhotoId,
    string PhotoPath,
    DateTime PhotoAddedOn,
    double Score,
    long? MessageId,
    long? ChannelId,
    DateTime? SentDate,
    string? MessageText,
    long? UserId,
    string? Username,
    string? FirstName,
    string? LastName);
