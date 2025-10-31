namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record TagSummaryDto(
    string Tag,
    int PhotoCount,
    long? UserId,
    string? Username,
    string? FirstName,
    string? LastName);
