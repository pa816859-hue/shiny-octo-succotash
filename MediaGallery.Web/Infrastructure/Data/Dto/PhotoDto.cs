namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record PhotoDto(
    long PhotoId,
    string FilePath,
    long AverageHash,
    long DifferenceHash,
    long PerceptualHash,
    DateTime AddedOn);
