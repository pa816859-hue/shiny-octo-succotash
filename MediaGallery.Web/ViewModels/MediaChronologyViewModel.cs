using System;
using System.Collections.Generic;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class MediaChronologyViewModel
{
    public MediaChronologyViewModel(
        MediaChronologyType mediaType,
        long mediaId,
        string? mediaPath,
        IReadOnlyList<RecentMessage> occurrences)
    {
        if (mediaId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mediaId));
        }

        MediaType = mediaType;
        MediaId = mediaId;
        MediaPath = mediaPath;
        Occurrences = occurrences ?? throw new ArgumentNullException(nameof(occurrences));
    }

    public MediaChronologyType MediaType { get; }

    public long MediaId { get; }

    public string? MediaPath { get; }

    public IReadOnlyList<RecentMessage> Occurrences { get; }

    public bool HasMedia => !string.IsNullOrWhiteSpace(MediaPath);

    public string Heading => MediaType == MediaChronologyType.Photo
        ? $"Photo #{MediaId}"
        : $"Video #{MediaId}";
}
