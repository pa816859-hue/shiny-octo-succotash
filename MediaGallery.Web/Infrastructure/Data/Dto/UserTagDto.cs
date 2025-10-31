namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record UserTagDto(
    long UserId,
    string Tag,
    int Weight);
