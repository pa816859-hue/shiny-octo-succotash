using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public interface IMessageService
{
    Task<RecentMessagesViewModel> GetRecentMessagesAsync(
        int pageNumber,
        int pageSize,
        long? channelId,
        long? userId,
        bool mediaOnly,
        MessageSortOrder sortOrder,
        CancellationToken cancellationToken = default);
}
