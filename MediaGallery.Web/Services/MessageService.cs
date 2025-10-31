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

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
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
        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);

        return trimmedItems.ToRecentMessagesViewModel(pagination);
    }
}
