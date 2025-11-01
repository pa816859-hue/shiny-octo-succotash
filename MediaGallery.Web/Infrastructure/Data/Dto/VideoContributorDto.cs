namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record VideoContributorDto(
    long UserId,
    string? Username,
    string? FirstName,
    string? LastName);
