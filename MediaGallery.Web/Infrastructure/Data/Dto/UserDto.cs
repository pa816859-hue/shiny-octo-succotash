namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record UserDto(
    long UserId,
    DateTime LastUpdate,
    string? FirstName,
    string? LastName,
    string? Username);
