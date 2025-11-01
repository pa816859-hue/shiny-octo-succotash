namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed record DashboardSummaryDto(
    long TotalMessages,
    long TotalPhotos,
    long TotalVideos,
    long ActiveChannels,
    long TotalUsers,
    DateTime? LastMessageSentAt);
