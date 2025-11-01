using System;
using System.Collections.Generic;
using System.Linq;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.ViewModels;

public sealed class UserProfileViewModel
{
    public UserProfileViewModel(UserProfile user, IReadOnlyList<RecentMessage> messages, PaginationMetadata pagination)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public UserProfile User { get; }

    public IReadOnlyList<RecentMessage> Messages { get; }

    public PaginationMetadata Pagination { get; }

    private IReadOnlyList<RecentMessage>? _textMessages;
    private IReadOnlyList<RecentMessage>? _photoMessages;
    private IReadOnlyList<RecentMessage>? _videoMessages;

    public IReadOnlyList<RecentMessage> TextMessages =>
        _textMessages ??= Messages
            .Where(message => !string.IsNullOrWhiteSpace(message.MessageText))
            .ToList();

    public IReadOnlyList<RecentMessage> PhotoMessages =>
        _photoMessages ??= Messages
            .Where(message =>
                message.PhotoId.HasValue &&
                !string.IsNullOrWhiteSpace(message.PhotoPath))
            .DistinctBy(message => message.PhotoId!.Value)
            .ToList();

    public IReadOnlyList<RecentMessage> VideoMessages =>
        _videoMessages ??= Messages
            .Where(message =>
                message.VideoId.HasValue &&
                !string.IsNullOrWhiteSpace(message.VideoPath))
            .DistinctBy(message => message.VideoId!.Value)
            .ToList();
}
