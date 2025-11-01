using System;

namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed class VideoDto
{
    public VideoDto(long videoId, string filePath, DateTime addedOn)
    {
        VideoId = videoId;
        FilePath = filePath;
        AddedOn = addedOn;
    }

    public long VideoId { get; }

    public string FilePath { get; }

    public DateTime AddedOn { get; }
}
