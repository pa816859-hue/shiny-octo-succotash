namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record PhotoTagDto(
    long PhotoId,
    string Tag,
    double Score);
