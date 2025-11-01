using System;
using System.Linq;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Services.Mapping;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMediaFileProvider _mediaFileProvider;

    public MessageService(IMessageRepository messageRepository, IMediaFileProvider mediaFileProvider)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _mediaFileProvider = mediaFileProvider ?? throw new ArgumentNullException(nameof(mediaFileProvider));
    }

    public async Task<RecentMessagesViewModel> GetRecentMessagesAsync(
        int pageNumber,
        int pageSize,
        long? channelId,
        long? userId,
        bool mediaOnly,
        MessageSortOrder sortOrder,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var offset = (pageNumber - 1) * pageSize;
        var fetchLimit = checked(pageSize + 1);
        var sortAscending = sortOrder == MessageSortOrder.OldestFirst;

        var items = await _messageRepository
            .GetRecentMessagesAsync(offset, fetchLimit, channelId, userId, sortAscending, mediaOnly, cancellationToken)
            .ConfigureAwait(false);

        var hasNextPage = items.Count > pageSize;
        var trimmedItems = hasNextPage ? items.Take(pageSize).ToList() : items.ToList();
        var normalizedItems = trimmedItems
            .Select(message => message with
            {
                PhotoPath = MediaPathFormatter.ToRelativeWebPath(message.PhotoPath, _mediaFileProvider.RootDirectory),
                VideoPath = MediaPathFormatter.ToRelativeWebPath(message.VideoPath, _mediaFileProvider.RootDirectory)
            })
            .ToList();
        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);

        return normalizedItems.ToRecentMessagesViewModel(pagination);
    }
}
