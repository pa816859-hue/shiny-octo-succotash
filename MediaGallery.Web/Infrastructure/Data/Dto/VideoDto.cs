using System;
using System.Collections.Generic;

namespace MediaGallery.Web.Infrastructure.Data.Dto;

public sealed class VideoDto
{
    private readonly List<VideoContributorDto> _contributors;

    public VideoDto(long videoId, string filePath, DateTime addedOn)
    {
        VideoId = videoId;
        FilePath = filePath;
        AddedOn = addedOn;
        _contributors = new List<VideoContributorDto>();
    }

    public long VideoId { get; }

    public string FilePath { get; }

    public DateTime AddedOn { get; }

    public IReadOnlyList<VideoContributorDto> Contributors => _contributors;

    public void SetContributors(IEnumerable<VideoContributorDto> contributors)
    {
        _contributors.Clear();

        if (contributors is null)
        {
            return;
        }

        _contributors.AddRange(contributors);
    }
}
